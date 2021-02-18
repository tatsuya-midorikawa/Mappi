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
#if NET40 || NET45 || NET46 || NET472 || NET48 || NETCOREAPP3_1 || NET5_0
    public sealed class MultipleDataReader2 : IDisposable, IDataReader
    {
        private SqlDataReader _reader;
        private bool _disposedValue;
        public bool HasNext { get; private set; }

        static MultipleDataReader2()
        {
        }

        public MultipleDataReader2(SqlDataReader reader)
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




        internal static class SetterCache<T>
        {
            private static Dictionary<string, Action<object, object>> _cache = new Dictionary<string, Action<object, object>>();

            public static bool Some => 0 < _cache.Count;

            public static void Add(string propertyName, Action<object, object> setter)
                => _cache.Add(propertyName, setter);

            public static bool TryGetSetter(string propertyName, out Action<object, object> setter)
                => _cache.TryGetValue(propertyName, out setter);
        }

        internal static class PropertyCache<T>
        {
            internal class Data
            {
                public Type Type { get; set; }
                public string Name { get; set; }
            }

            private static List<Data> _cache = new List<Data>();

            public static bool Some => 0 < _cache.Count;

            public static void Add(Data info)
                => _cache.Add(info);

            public static IEnumerable<Data> Enumerate()
                => _cache;
        }

        static class NoneGetterCache
        {
            private static Dictionary<Type, Func<object>> _cache = new Dictionary<Type, Func<object>>();

            public static void Add(Type type, Func<object> getter)
                => _cache.Add(type, getter);

            public static bool TryGetNoneGetter(Type type, out Func<object> getter)
                => _cache.TryGetValue(type, out getter);
        }

        static class SomeMethodCache
        {
            private static Dictionary<Type, Func<object, object>> _cache = new Dictionary<Type, Func<object, object>>();

            public static void Add(Type type, Func<object, object> method)
                => _cache.Add(type, method);

            public static bool TryGetSomeMethod(Type type, out Func<object, object> method)
                => _cache.TryGetValue(type, out method);
        }

        static class DiscriminatedUnionsConstructorCache
        {
            private static Dictionary<Type, Dictionary<Type, Func<object, object>>> _cache 
                = new Dictionary<Type, Dictionary<Type, Func<object, object>>>();

            public static void Add(Type duType, Type valueType, Func<object, object> method)
            { 
                if (_cache.TryGetValue(duType, out Dictionary<Type, Func<object, object>> duCache))
                {
                    if (duCache.TryGetValue(valueType, out Func<object, object> _))
                        return;
                    else
                        duCache.Add(valueType, method);
                }
                else
                {
                    _cache.Add(duType, new Dictionary<Type, Func<object, object>>());
                    _cache[duType].Add(valueType, method);
                }
            }

            public static bool TryGetSomeMethod(Type duType, Type valueType, out Func<object, object> method)
            {
                method = null;
                return _cache.TryGetValue(duType, out Dictionary<Type, Func<object, object>> cache)
                        && cache.TryGetValue(valueType, out method);
            }
        }






        public IEnumerable<T> Read<T>() where T : new()
        {
            if (!HasNext)
                throw new Exception("The data has already been loaded.");

            // setterを作成する
            if (!PropertyCache<T>.Some || !SetterCache<T>.Some)
            {
                foreach (var property in typeof(T)
                    .GetProperties((BindingFlags.Instance | BindingFlags.Public) ^ BindingFlags.DeclaredOnly)
                    .Where(property => property.GetAttribute<IgnoreAttribute>() == null))
                {
                    var columnName = property.GetAttribute<ColumnAttribute>() is ColumnAttribute c ? c.Name : property.Name;
                    var data = new PropertyCache<T>.Data{ Name = columnName, Type = property.PropertyType };
                    PropertyCache<T>.Add(data);
                    SetterCache<T>.Add(data.Name, BuildSetter(property));
                }
            }

            while (_reader.Read())
            {
                object instance = new T();

                foreach (var property in PropertyCache<T>.Enumerate())
                {
                    if (SetterCache<T>.TryGetSetter(property.Name, out Action<object, object> setter))
                    {
                        var value = Resolve(property.Type, _reader[property.Name]);
                        setter(instance, value);
                    }
                }

                yield return (T)instance;
            }

            HasNext = _reader.NextResult();
        }

        private static Action<object, object> BuildSetter(PropertyInfo propertyInfo)
        {
            var method = propertyInfo.GetSetMethod(true);
            var target = Expression.Parameter(typeof(object), "target");
            var value = Expression.Parameter(typeof(object), "value");

            var expr =
                Expression.Lambda<Action<object, object>>(
                    Expression.Call(
                        propertyInfo.DeclaringType.IsValueType ?
                            Expression.Unbox(target, method.DeclaringType) :
                            Expression.Convert(target, method.DeclaringType),
                        method,
                        Expression.Convert(value, method.GetParameters()[0].ParameterType)),
                        target, value);
            return expr.Compile();
        }

        private static Func<object> BuildNoneGetter(Type type)
        {
            if (!IsFsharpOption(type))
                throw new ArgumentException("'type' is not FSharpOption<'T> type.");

            if (NoneGetterCache.TryGetNoneGetter(type, out Func<object> getter))
                return getter;

            var propertyInfo = type.GetProperty("None");
            getter = Expression.Lambda<Func<object>>(
                Expression.MakeMemberAccess(null, propertyInfo)
            ).Compile();
            NoneGetterCache.Add(type, getter);
            return getter;
        }

        private static Func<object, object> BuildSomeMethod(Type type)
        {
            if (!IsFsharpOption(type))
                throw new ArgumentException("'type' is not FSharpOption<'T> type.");

            if (SomeMethodCache.TryGetSomeMethod(type, out Func<object, object> someMethod))
                return someMethod;

            var some = type.GetMethod("Some");
            var value = Expression.Parameter(typeof(object), "value");
            someMethod = Expression.Lambda<Func<object, object>>(
                Expression.Call(
                    null,
                    some,
                    Expression.Convert(value, some.GetParameters()[0].ParameterType)),
                value
                ).Compile();
            SomeMethodCache.Add(type, someMethod);
            return someMethod;
        }

        private static Func<object, object> BuildDiscriminatedUnionsConstructor(Type duType, Type valueType)
        {
            if (DiscriminatedUnionsConstructorCache.TryGetSomeMethod(duType, valueType, out Func<object, object> ctor))
                return ctor;

            var method = duType
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m =>
                {
                    var ps = m.GetParameters();
                    return ps.Length == 1 && ps[0].ParameterType == valueType;
                });

            if (method == null)
                throw new ArgumentException($"There are no cases of discriminant unions that are '{valueType.FullName}' type only.");

            var value = Expression.Parameter(typeof(object), "value");
            ctor = Expression.Lambda<Func<object, object>>(
                Expression.Call(
                    null,
                    method,
                    Expression.Convert(value, method.GetParameters()[0].ParameterType)),
                value
                ).Compile();
            DiscriminatedUnionsConstructorCache.Add(duType, valueType, ctor);
            return ctor;
        }

        private static bool IsFsharpOption(Type type)
        {
            return type.IsGenericType
                && !type.IsGenericTypeDefinition
                && !type.IsGenericParameter
                && typeof(FSharpOption<>) == type.GetGenericTypeDefinition();
        }

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

            if (memberType == typeof(int))
                return value is DBNull ? default(int) : value;
            if (memberType == typeof(uint))
                return value is DBNull ? default(uint) : value;
            if (memberType == typeof(short))
                return value is DBNull ? default(short) : value;
            if (memberType == typeof(ushort))
                return value is DBNull ? default(ushort) : value;
            if (memberType == typeof(long))
                return value is DBNull ? default(long) : value;
            if (memberType == typeof(ulong))
                return value is DBNull ? default(ulong) : value;
            if (memberType == typeof(byte))
                return value is DBNull ? default(byte) : value;
            if (memberType == typeof(sbyte))
                return value is DBNull ? default(sbyte) : value;
            if (memberType == typeof(bool))
                return value is DBNull ? default(bool) : value;
            if (memberType == typeof(float))
                return value is DBNull ? default(float) : value;
            if (memberType == typeof(double))
                return value is DBNull ? default(double) : value;
            if (memberType == typeof(decimal))
                return value is DBNull ? default(decimal) : value;
            if (memberType == typeof(char))
                return value is DBNull ? default(char) : value;
            if (memberType == typeof(string))
                return value is DBNull ? default(string) : value;
            if (memberType == typeof(Guid))
                return value is DBNull ? default(Guid) : value;
            if (memberType == typeof(DateTime))
                return value is DBNull ? default(DateTime) : value;
            if (memberType == typeof(DateTimeOffset))
                return value is DBNull ? default(DateTimeOffset) : value;
            if (memberType == typeof(byte[]))
                return value;

            if (memberType == typeof(int?))
                return value is DBNull ? default(int?) : value;
            if (memberType == typeof(uint?))
                return value is DBNull ? default(uint?) : value;
            if (memberType == typeof(short?))
                return value is DBNull ? default(short?) : value;
            if (memberType == typeof(ushort?))
                return value is DBNull ? default(ushort?) : value;
            if (memberType == typeof(long?))
                return value is DBNull ? default(long?) : value;
            if (memberType == typeof(ulong?))
                return value is DBNull ? default(ulong?) : value;
            if (memberType == typeof(byte?))
                return value is DBNull ? default(byte?) : value;
            if (memberType == typeof(sbyte?))
                return value is DBNull ? default(sbyte?) : value;
            if (memberType == typeof(bool?))
                return value is DBNull ? default(bool?) : value;
            if (memberType == typeof(float?))
                return value is DBNull ? default(float?) : value;
            if (memberType == typeof(double?))
                return value is DBNull ? default(double?) : value;
            if (memberType == typeof(decimal?))
                return value is DBNull ? default(decimal?) : value;
            if (memberType == typeof(char?))
                return value is DBNull ? default(char?) : value;
            if (memberType == typeof(Guid?))
                return value is DBNull ? default(Guid?) : value;
            if (memberType == typeof(DateTime?))
                return value is DBNull ? default(DateTime?) : value;
            if (memberType == typeof(DateTimeOffset?))
                return value is DBNull ? default(DateTimeOffset?) : value;

            if (memberType == typeof(FSharpOption<int>))
                return value is DBNull ? FSharpOption<int>.None : FSharpOption<int>.Some((int)value);
            if (memberType == typeof(FSharpOption<uint>))
                return value is DBNull ? FSharpOption<uint>.None : FSharpOption<uint>.Some((uint)value);
            if (memberType == typeof(FSharpOption<short>))
                return value is DBNull ? FSharpOption<short>.None : FSharpOption<short>.Some((short)value);
            if (memberType == typeof(FSharpOption<ushort>))
                return value is DBNull ? FSharpOption<ushort>.None : FSharpOption<ushort>.Some((ushort)value);
            if (memberType == typeof(FSharpOption<long>))
                return value is DBNull ? FSharpOption<long>.None : FSharpOption<long>.Some((long)value);
            if (memberType == typeof(FSharpOption<ulong>))
                return value is DBNull ? FSharpOption<ulong>.None : FSharpOption<ulong>.Some((ulong)value);
            if (memberType == typeof(FSharpOption<byte>))
                return value is DBNull ? FSharpOption<byte>.None : FSharpOption<byte>.Some((byte)value);
            if (memberType == typeof(FSharpOption<sbyte>))
                return value is DBNull ? FSharpOption<sbyte>.None : FSharpOption<sbyte>.Some((sbyte)value);
            if (memberType == typeof(FSharpOption<bool>))
                return value is DBNull ? FSharpOption<bool>.None : FSharpOption<bool>.Some((bool)value);
            if (memberType == typeof(FSharpOption<float>))
                return value is DBNull ? FSharpOption<float>.None : FSharpOption<float>.Some((float)value);
            if (memberType == typeof(FSharpOption<double>))
                return value is DBNull ? FSharpOption<double>.None : FSharpOption<double>.Some((double)value);
            if (memberType == typeof(FSharpOption<decimal>))
                return value is DBNull ? FSharpOption<decimal>.None : FSharpOption<decimal>.Some((decimal)value);
            if (memberType == typeof(FSharpOption<char>))
                return value is DBNull ? FSharpOption<char>.None : FSharpOption<char>.Some((char)value);
            if (memberType == typeof(FSharpOption<string>))
                return value is DBNull ? FSharpOption<string>.None : FSharpOption<string>.Some((string)value);
            if (memberType == typeof(FSharpOption<Guid>))
                return value is DBNull ? FSharpOption<Guid>.None : FSharpOption<Guid>.Some((Guid)value);
            if (memberType == typeof(FSharpOption<DateTime>))
                return value is DBNull ? FSharpOption<DateTime>.None : FSharpOption<DateTime>.Some((DateTime)value);
            if (memberType == typeof(FSharpOption<DateTimeOffset>))
                return value is DBNull ? FSharpOption<DateTimeOffset>.None : FSharpOption<DateTimeOffset>.Some((DateTimeOffset)value);
            if (memberType == typeof(FSharpOption<byte[]>))
                return value is DBNull ? FSharpOption<byte[]>.None : FSharpOption<byte[]>.Some((byte[])value);

            // optional values (F#)
            if (IsFsharpOption(memberType))
            {
                if (value is DBNull)
                    return BuildNoneGetter(memberType)();

                var genericTypes = memberType.GetGenericArguments();
                if (genericTypes.Length != 1)
                    throw new Exception("Invalid fsharp option value.");

                var genericType = genericTypes[0];
                return BuildSomeMethod(memberType)(Resolve(genericType, value));
            }

            // discriminated unions (F#)
            if (FSharpType.IsUnion(memberType, none))
            {
                return BuildDiscriminatedUnionsConstructor(memberType, value?.GetType())(value);
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
#endif
}
