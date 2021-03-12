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
using Mappi.Resolvers;

#if NET40 || NET45 || NET46 || NET472 || NET48 || NETCOREAPP3_1 || NET5_0
using System.Collections.Concurrent;
#endif

#if NET45 || NET46 || NET472 || NET48 || NETCOREAPP3_1 || NET5_0
using System.Threading.Tasks;
#endif

namespace Mappi
{

    public sealed class MultipleDataReader : IDisposable
    {
        private SqlDataReader _reader;
        private bool _disposedValue;
        private readonly IDataResolver _resolver;
        public bool HasNext { get; private set; }

        static MultipleDataReader()
        {
        }

        public MultipleDataReader(SqlDataReader reader, IDataResolver resolver = null)
        {
            _reader = reader;
            _disposedValue = false;
            _resolver = resolver == null ? new DefaultDataResolver() : resolver;
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

#if NET40 || NET45 || NET46 || NET472 || NET48 || NETCOREAPP3_1 || NET5_0
        public IEnumerable<T> Read<T>()
        {
            if (!HasNext)
                throw new Exception("The data has already been loaded.");

            var isFsharpRecordType = FSharpType.IsRecord(typeof(T), FSharpOption<BindingFlags>.None);

            // F# record型の場合とそれ以外とで処理を分岐する.
            //   -> F# recordは引数なしのデフォルトコンストラクタがないため.
            if (FSharpType.IsRecord(typeof(T), FSharpOption<BindingFlags>.None))
            {
                // コンストラクタの取得
                var ctor = BuildConstructor(typeof(T), false);
                if (!_constructorArgsInfoCache.TryGetValue(typeof(T), out ConstructorArgsInfo[] keys))
                    throw new ArgumentException("T is invalid record type");

                var args = new object[keys.Length];
                while (_reader.Read())
                {
                    for (var i = 0; i < keys.Length; i++)
                        args[i] = _resolver.Resolve(keys[i].Type, _reader[keys[i].Name]);

                    yield return (T)ctor(args);
                }
            }
            else
            {
                // setterを作成する
                if (!PropertyCache<T>.Some || !SetterCache<T>.Some)
                {
                    foreach (var property in typeof(T)
                        .GetProperties((BindingFlags.Instance | BindingFlags.Public) ^ BindingFlags.DeclaredOnly)
                        .Where(property => property.GetAttribute<IgnoreAttribute>() == null))
                    {
                        var columnName = property.GetAttribute<ColumnAttribute>() is ColumnAttribute c ? c.Name : property.Name;
                        var data = new PropertyCache<T>.Data { Name = columnName, Type = property.PropertyType };
                        PropertyCache<T>.Add(data);
                        SetterCache<T>.GetOrAdd(data.Name, BuildSetter(property));
                    }
                }

                while (_reader.Read())
                {
                    object instance = BuildConstructor(typeof(T))(null);

                    foreach (var property in PropertyCache<T>.Enumerate())
                    {
                        if (SetterCache<T>.TryGetSetter(property.Name, out Action<object, object> setter))
                        {
                            var value = _resolver.Resolve(property.Type, _reader[property.Name]);
                            setter(instance, value);
                        }
                    }

                    yield return (T)instance;
                }
            }


            HasNext = _reader.NextResult();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static Func<object[], object> BuildConstructor(Type type, bool useDefaultConstructor = true)
        {
            if (_constructorCache.TryGetValue(type, out Func<object[], object> ctor))
                return ctor;

            var args = Expression.Parameter(typeof(object[]), "args");
            var ctorInfos = type.GetConstructors();
            var ctorInfo = useDefaultConstructor
                ? ctorInfos.FirstOrDefault(ci => ci.GetParameters().Length == 0)
                : ctorInfos.FirstOrDefault();

            if (ctorInfo == null)
                throw new ArgumentException("The constructor is not available.");

            var parameters = ctorInfo.GetParameters();
            var parametersExpr = parameters
                .Select((x, i) =>
                    Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(i)),
                    x.ParameterType))
                .ToArray();

            ctor = Expression.Lambda<Func<object[], object>>(
                Expression.Convert(
                    Expression.New(ctorInfo, parametersExpr),
                    typeof(object)),
                args).Compile();

            // F# record型の場合のみ, コンストラクタの引数情報をキャッシュする
            if (FSharpType.IsRecord(type, FSharpOption<BindingFlags>.None))
            {
                var keys = parameters.Select(p => p.Name).ToArray();
                var props = type
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.GetAttribute<CompilationMappingAttribute>() != null)
                    .OrderBy(p => p.GetAttribute<CompilationMappingAttribute>().SequenceNumber);

                foreach (var prop in props)
                {
                    var mapping = prop.GetAttribute<CompilationMappingAttribute>();
                    keys[mapping.SequenceNumber] = prop.GetAttribute<ColumnAttribute>() is ColumnAttribute colmun
                        ? colmun.Name
                        : prop.Name;
                }

                var data = new ConstructorArgsInfo[keys.Length];
                for (var i = 0; i < parameters.Length; i++)
                    data[i] = new ConstructorArgsInfo { Type = parameters[i].ParameterType, Name = keys[i] };
                _constructorArgsInfoCache.GetOrAdd(type, data);
            }

            return _constructorCache.GetOrAdd(type, ctor);
        }
        private static ConcurrentDictionary<Type, Func<object[], object>> _constructorCache = new ConcurrentDictionary<Type, Func<object[], object>>();
        private static ConcurrentDictionary<Type, ConstructorArgsInfo[]> _constructorArgsInfoCache = new ConcurrentDictionary<Type, ConstructorArgsInfo[]>();


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private static class SetterCache<T>
        {
            private static ConcurrentDictionary<string, Action<object, object>> _cache
                = new ConcurrentDictionary<string, Action<object, object>>();

            public static bool Some => 0 < _cache.Count;

            public static Action<object, object> GetOrAdd(string propertyName, Action<object, object> setter)
                => _cache.GetOrAdd(propertyName, setter);

            public static bool TryGetSetter(string propertyName, out Action<object, object> setter)
                => _cache.TryGetValue(propertyName, out setter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private static class PropertyCache<T>
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

        private class ConstructorArgsInfo
        {
            public Type Type { get; set; }
            public string Name { get; set; }
        }
#else
        public IEnumerable<T> Read<T>()
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
                    var value = _resolver.Resolve(property.PropertyType, _reader[columnName]);
                    property.GetBackingField().SetValueDirect(__makeref(instance), value);
                }

                yield return (T)instance;
            }

            HasNext = _reader.NextResult();
        }
#endif

#if NET45 || NET46 || NET472 || NET48 || NETCOREAPP3_1 || NET5_0
        public async Task<IEnumerable<T>> ReadAsync<T>(int baseCapacity = 128)
        {
            if (!HasNext)
                throw new Exception("The data has already been loaded.");

            var acc = new List<T>(baseCapacity);
            var isFsharpRecordType = FSharpType.IsRecord(typeof(T), FSharpOption<BindingFlags>.None);

            // F# record型の場合とそれ以外とで処理を分岐する.
            //   -> F# recordは引数なしのデフォルトコンストラクタがないため.
            if (FSharpType.IsRecord(typeof(T), FSharpOption<BindingFlags>.None))
            {
                // コンストラクタの取得
                var ctor = BuildConstructor(typeof(T), false);
                if (!_constructorArgsInfoCache.TryGetValue(typeof(T), out ConstructorArgsInfo[] keys))
                    throw new ArgumentException("T is invalid record type");

                var args = new object[keys.Length];
                while (await _reader.ReadAsync())
                {
                    for (var i = 0; i < keys.Length; i++)
                        args[i] = _resolver.Resolve(keys[i].Type, _reader[keys[i].Name]);

                    acc.Add((T)ctor(args));
                }
            }
            else
            {
                // setterを作成する
                if (!PropertyCache<T>.Some || !SetterCache<T>.Some)
                {
                    foreach (var property in typeof(T)
                        .GetProperties((BindingFlags.Instance | BindingFlags.Public) ^ BindingFlags.DeclaredOnly)
                        .Where(property => property.GetAttribute<IgnoreAttribute>() == null))
                    {
                        var columnName = property.GetAttribute<ColumnAttribute>() is ColumnAttribute c ? c.Name : property.Name;
                        var data = new PropertyCache<T>.Data { Name = columnName, Type = property.PropertyType };
                        PropertyCache<T>.Add(data);
                        SetterCache<T>.GetOrAdd(data.Name, BuildSetter(property));
                    }
                }

                while (await _reader.ReadAsync())
                {
                    object instance = BuildConstructor(typeof(T))(null);

                    foreach (var property in PropertyCache<T>.Enumerate())
                    {
                        if (SetterCache<T>.TryGetSetter(property.Name, out Action<object, object> setter))
                        {
                            var value = _resolver.Resolve(property.Type, _reader[property.Name]);
                            setter(instance, value);
                        }
                    }

                    acc.Add((T)instance);
                }
            }

            HasNext = _reader.NextResult();
            return acc;
        }
#endif
    }
}
