using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Reffy;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Reflection;
using Mono.Reflection;
using System.Linq.Expressions;

#if NET45 || NET46 || NET472 || NET48 || NETCOREAPP3_1 || NET5_0
using System.Threading.Tasks;
#endif

namespace Mappi
{
    public sealed class MultipleDataReader : IDisposable, IDataReader
    {
        private SqlDataReader _reader;
        private bool _disposedValue;

        public bool HasNext { get; private set; }

        static MultipleDataReader()
        {
        }


        public MultipleDataReader(SqlDataReader reader)
        {
            _reader = reader;
            _disposedValue = false;
            HasNext = true;
        }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _reader?.Dispose();
                }

                _reader = null;
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

//#if NET35
        public IEnumerable<T> Read<T>() where T : new()
        {
            if (!HasNext)
                throw new Exception("The data has already been loaded.");

            var type = typeof(T);

            while (_reader.Read())
            {
                var instance = type.MakeDefault();

                foreach (var property in type
                       .GetProperties((BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ^ BindingFlags.DeclaredOnly)
                       .Where(property => property.GetAttribute<IgnoreAttribute>() == null))
                {
                    var columnName = property.GetAttribute<ColumnAttribute>() is ColumnAttribute c ? c.Name : property.Name;
                    var value = Resolve(property.PropertyType, _reader[columnName]);
                    property.GetBackingField().SetValueDirect(__makeref(instance), value);
                }

                yield return (T)instance;
            }

            HasNext = _reader.NextResult();
        }

//#elif NET40 || NET45 || NET46 || NET472 || NET48 || NETCOREAPP3_1 || NET5_0

//        public IEnumerable<T> Read<T>() where T : new()
//        {
//            if (!HasNext)
//                throw new Exception("The data has already been loaded.");

//            if (!PropertyCache<T>.Some || !SetterCache<T>.Some)
//            {
//                foreach (var property in typeof(T)
//                    .GetProperties((BindingFlags.Instance | BindingFlags.Public) ^ BindingFlags.DeclaredOnly)
//                    .Where(property => property.GetAttribute<IgnoreAttribute>() == null))
//                {
//                    var columnName = property.GetAttribute<ColumnAttribute>() is ColumnAttribute c ? c.Name : property.Name;
//                    var data = new PropertyCache<T>.Data { Name = columnName, Type = property.PropertyType };
//                    PropertyCache<T>.Add(data);
//                    SetterCache<T>.Add(data.Name, BuildSetter(property));
//                }
//            }

//            while (_reader.Read())
//            {
//                object instance = new T();

//                foreach (var property in PropertyCache<T>.Enumerate())
//                {
//                    if (SetterCache<T>.TryGetSetter(property.Name, out Action<object, object> setter))
//                    {
//                        var value = Resolve(property.Type, _reader[property.Name]);
//                        setter(instance, value);
//                    }
//                }

//                yield return (T)instance;
//            }

//            HasNext = _reader.NextResult();
//        }

//        internal static class SetterCache<T>
//        {
//            private static Dictionary<string, Action<object, object>> _cache = new Dictionary<string, Action<object, object>>();

//            public static bool Some => 0 < _cache.Count;

//            public static void Add(string propertyName, Action<object, object> setter)
//                => _cache.Add(propertyName, setter);

//            public static bool TryGetSetter(string propertyName, out Action<object, object> setter)
//                => _cache.TryGetValue(propertyName, out setter);
//        }

//        internal static class PropertyCache<T>
//        {
//            internal class Data
//            {
//                public Type Type { get; set; }
//                public string Name { get; set; }
//            }

//            private static List<Data> _cache = new List<Data>();

//            public static bool Some => 0 < _cache.Count;

//            public static void Add(Data info)
//                => _cache.Add(info);

//            public static IEnumerable<Data> Enumerate()
//                => _cache;
//        }

//        private static Action<object, object> BuildSetter(PropertyInfo propertyInfo)
//        {
//            var method = propertyInfo.GetSetMethod(true);
//            var target = Expression.Parameter(typeof(object), "target");
//            var value = Expression.Parameter(typeof(object), "value");

//            var expr =
//                Expression.Lambda<Action<object, object>>(
//                    Expression.Call(
//                        propertyInfo.DeclaringType.IsValueType ?
//                            Expression.Unbox(target, method.DeclaringType) :
//                            Expression.Convert(target, method.DeclaringType),
//                        method,
//                        Expression.Convert(value, method.GetParameters()[0].ParameterType)),
//                        target, value);
//            return expr.Compile();
//        }
//#endif

#if NET45 || NET46 || NET472 || NET48 || NETCOREAPP3_1 || NET5_0
        public async Task<IEnumerable<T>> ReadAsync<T>(int baseCapacity = 128)
            where T : class
        {
            if (!HasNext)
                throw new Exception("The data has already been loaded.");

            var type = typeof(T);
            var acc = new List<T>(baseCapacity);

            while (await _reader.ReadAsync())
            {
                var instance = type.MakeDefault();

                foreach (var property in type
                       .GetProperties((BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ^ BindingFlags.DeclaredOnly)
                       .Where(property => property.GetAttribute<IgnoreAttribute>() == null))
                {
                    var columnName = property.GetAttribute<ColumnAttribute>() is ColumnAttribute c ? c.Name : property.Name;
                    var value = Resolve(property.PropertyType, _reader[columnName]);
                    property.GetBackingField().SetValue(instance, value);
                }

                acc.Add((T)instance);
            }

            HasNext = _reader.NextResult();
            return acc;
        }
#endif

        private static object Resolve(Type memberType, object value)
        {
            var none = FSharpOption<BindingFlags>.None;

            // optional values (F#)
            if (memberType.IsGenericType
                && !memberType.IsGenericTypeDefinition
                && !memberType.IsGenericParameter
                && typeof(FSharpOption<>) == memberType.GetGenericTypeDefinition())
            {
                if (value is DBNull)
                    return memberType.GetProperty("None", BindingFlags.Public | BindingFlags.Static).GetGetMethod().Invoke(null, null);

                var genericTypes = memberType.GetGenericArguments();
                if (genericTypes.Length != 1)
                    throw new Exception("Invalid fsharp option value.");

                var genericType = genericTypes[0];
                return memberType.GetMethod("Some", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { Resolve(genericType, value) });
            }

            // discriminated unions (F#)
            if (FSharpType.IsUnion(memberType, none))
            {
                var ctor = FSharpType.GetUnionCases(memberType, none).FirstOrDefault(cinfo => cinfo.GetFields().Length == 1);
                if (ctor == null)
                    throw new Exception("Invalid discriminated-unions.");

                var field = ctor.GetFields()[0];
                return FSharpValue.MakeUnion(ctor, new object[] { Resolve(field.PropertyType, value) }, none);
            }

            // nullable values
            if (memberType.IsGenericType
                && !memberType.IsGenericTypeDefinition
                && !memberType.IsGenericParameter
                && typeof(Nullable<>) == memberType.GetGenericTypeDefinition())
            {
                return value is DBNull ? null : value;
            }

            // non nullable values
            if (memberType.IsValueType)
            {
                return value is DBNull ? Activator.CreateInstance(memberType) : value;
            }

            // class values
            if (memberType.IsClass)
            {
                if (value is string)
                    return value;

                return value is DBNull ? null : Activator.CreateInstance(memberType, value);
            }

            // enums
            if (memberType.IsEnum)
            {
                if (value is DBNull)
                    return memberType.GetEnumValue(0);

                if (value is short s)
                    return memberType.GetEnumValue(s);
                if (value is int i)
                    return memberType.GetEnumValue(i);
                if (value is long l)
                    return memberType.GetEnumValue(l);

                if (value is ushort us)
                    return memberType.GetEnumValue(us);
                if (value is uint ui)
                    return memberType.GetEnumValue(ui);
                if (value is ulong ul)
                    return memberType.GetEnumValue(ul);

                if (value is byte b)
                    return memberType.GetEnumValue(b);
                if (value is sbyte sb)
                    return memberType.GetEnumValue(sb);
            }

            throw new Exception("Could not resolve the value.");
        }
    }
}
