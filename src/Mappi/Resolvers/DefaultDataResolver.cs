#if NET40 || NET45 || NET46 || NET472 || NET48 || NETCOREAPP3_1 || NET5_0
using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Collections.Concurrent;

namespace Mappi.Resolvers
{
    public class DefaultDataResolver : IDataResolver
    {
        public object Resolve(Type propertyType, object value)
        {
            var none = FSharpOption<BindingFlags>.None;

            if (propertyType == typeof(int))
                return value is DBNull ? default(int) : value;
            if (propertyType == typeof(uint))
                return value is DBNull ? default(uint) : value;
            if (propertyType == typeof(short))
                return value is DBNull ? default(short) : value;
            if (propertyType == typeof(ushort))
                return value is DBNull ? default(ushort) : value;
            if (propertyType == typeof(long))
                return value is DBNull ? default(long) : value;
            if (propertyType == typeof(ulong))
                return value is DBNull ? default(ulong) : value;
            if (propertyType == typeof(byte))
                return value is DBNull ? default(byte) : value;
            if (propertyType == typeof(sbyte))
                return value is DBNull ? default(sbyte) : value;
            if (propertyType == typeof(bool))
                return value is DBNull ? default(bool) : value;
            if (propertyType == typeof(float))
                return value is DBNull ? default(float) : value;
            if (propertyType == typeof(double))
                return value is DBNull ? default(double) : value;
            if (propertyType == typeof(decimal))
                return value is DBNull ? default(decimal) : value;
            if (propertyType == typeof(char))
                return value is DBNull ? default(char) : value;
            if (propertyType == typeof(string))
                return value is DBNull ? default(string) : value;
            if (propertyType == typeof(Guid))
                return value is DBNull ? default(Guid) : value;
            if (propertyType == typeof(DateTime))
                return value is DBNull ? default(DateTime) : value;
            if (propertyType == typeof(DateTimeOffset))
                return value is DBNull ? default(DateTimeOffset) : value;
            if (propertyType == typeof(byte[]))
                return value;

            if (propertyType == typeof(int?))
                return value is DBNull ? default(int?) : value;
            if (propertyType == typeof(uint?))
                return value is DBNull ? default(uint?) : value;
            if (propertyType == typeof(short?))
                return value is DBNull ? default(short?) : value;
            if (propertyType == typeof(ushort?))
                return value is DBNull ? default(ushort?) : value;
            if (propertyType == typeof(long?))
                return value is DBNull ? default(long?) : value;
            if (propertyType == typeof(ulong?))
                return value is DBNull ? default(ulong?) : value;
            if (propertyType == typeof(byte?))
                return value is DBNull ? default(byte?) : value;
            if (propertyType == typeof(sbyte?))
                return value is DBNull ? default(sbyte?) : value;
            if (propertyType == typeof(bool?))
                return value is DBNull ? default(bool?) : value;
            if (propertyType == typeof(float?))
                return value is DBNull ? default(float?) : value;
            if (propertyType == typeof(double?))
                return value is DBNull ? default(double?) : value;
            if (propertyType == typeof(decimal?))
                return value is DBNull ? default(decimal?) : value;
            if (propertyType == typeof(char?))
                return value is DBNull ? default(char?) : value;
            if (propertyType == typeof(Guid?))
                return value is DBNull ? default(Guid?) : value;
            if (propertyType == typeof(DateTime?))
                return value is DBNull ? default(DateTime?) : value;
            if (propertyType == typeof(DateTimeOffset?))
                return value is DBNull ? default(DateTimeOffset?) : value;

            if (propertyType == typeof(FSharpOption<int>))
                return value is DBNull ? FSharpOption<int>.None : FSharpOption<int>.Some((int)value);
            if (propertyType == typeof(FSharpOption<uint>))
                return value is DBNull ? FSharpOption<uint>.None : FSharpOption<uint>.Some((uint)value);
            if (propertyType == typeof(FSharpOption<short>))
                return value is DBNull ? FSharpOption<short>.None : FSharpOption<short>.Some((short)value);
            if (propertyType == typeof(FSharpOption<ushort>))
                return value is DBNull ? FSharpOption<ushort>.None : FSharpOption<ushort>.Some((ushort)value);
            if (propertyType == typeof(FSharpOption<long>))
                return value is DBNull ? FSharpOption<long>.None : FSharpOption<long>.Some((long)value);
            if (propertyType == typeof(FSharpOption<ulong>))
                return value is DBNull ? FSharpOption<ulong>.None : FSharpOption<ulong>.Some((ulong)value);
            if (propertyType == typeof(FSharpOption<byte>))
                return value is DBNull ? FSharpOption<byte>.None : FSharpOption<byte>.Some((byte)value);
            if (propertyType == typeof(FSharpOption<sbyte>))
                return value is DBNull ? FSharpOption<sbyte>.None : FSharpOption<sbyte>.Some((sbyte)value);
            if (propertyType == typeof(FSharpOption<bool>))
                return value is DBNull ? FSharpOption<bool>.None : FSharpOption<bool>.Some((bool)value);
            if (propertyType == typeof(FSharpOption<float>))
                return value is DBNull ? FSharpOption<float>.None : FSharpOption<float>.Some((float)value);
            if (propertyType == typeof(FSharpOption<double>))
                return value is DBNull ? FSharpOption<double>.None : FSharpOption<double>.Some((double)value);
            if (propertyType == typeof(FSharpOption<decimal>))
                return value is DBNull ? FSharpOption<decimal>.None : FSharpOption<decimal>.Some((decimal)value);
            if (propertyType == typeof(FSharpOption<char>))
                return value is DBNull ? FSharpOption<char>.None : FSharpOption<char>.Some((char)value);
            if (propertyType == typeof(FSharpOption<string>))
                return value is DBNull ? FSharpOption<string>.None : FSharpOption<string>.Some((string)value);
            if (propertyType == typeof(FSharpOption<Guid>))
                return value is DBNull ? FSharpOption<Guid>.None : FSharpOption<Guid>.Some((Guid)value);
            if (propertyType == typeof(FSharpOption<DateTime>))
                return value is DBNull ? FSharpOption<DateTime>.None : FSharpOption<DateTime>.Some((DateTime)value);
            if (propertyType == typeof(FSharpOption<DateTimeOffset>))
                return value is DBNull ? FSharpOption<DateTimeOffset>.None : FSharpOption<DateTimeOffset>.Some((DateTimeOffset)value);
            if (propertyType == typeof(FSharpOption<byte[]>))
                return value is DBNull ? FSharpOption<byte[]>.None : FSharpOption<byte[]>.Some((byte[])value);

            // optional values (F#)
            if (IsFsharpOption(propertyType))
            {
                if (value is DBNull)
                    return BuildNoneGetter(propertyType)();

                var genericTypes = propertyType.GetGenericArguments();
                if (genericTypes.Length != 1)
                    throw new Exception("Invalid fsharp option value.");

                var genericType = genericTypes[0];
                return BuildSomeMethod(propertyType)(Resolve(genericType, value));
            }

            // discriminated unions (F#)
            if (FSharpType.IsUnion(propertyType, none))
            {
                if (!(_duConstructorParamTypeCache.TryGetValue(propertyType, out Type paramType)))
                {
                    var valueinfo = GetDiscriminatedUnionsConstructorInfo(propertyType, value?.GetType());
                    paramType = valueinfo.GetParameters()[0].ParameterType;
                }
                return BuildDiscriminatedUnionsConstructor(propertyType, paramType)(Resolve(paramType, value));
            }

            // nullable values
            if (IsNullable(propertyType))
            {
                return value is DBNull ? null : value;
            }

            // non nullable values
            if (propertyType.IsValueType)
            {
                return value is DBNull ? Activator.CreateInstance(propertyType) : value;
            }

            // class values
            if (propertyType.IsClass)
            {
                return value is DBNull ? null : Activator.CreateInstance(propertyType, value);
            }

            // enums
            if (propertyType.IsEnum)
            {
                if (value is DBNull)
                    return propertyType.GetEnumValue(0);

                if (value is short s)
                    return propertyType.GetEnumValue(s);
                if (value is int i)
                    return propertyType.GetEnumValue(i);
                if (value is long l)
                    return propertyType.GetEnumValue(l);

                if (value is ushort us)
                    return propertyType.GetEnumValue(us);
                if (value is uint ui)
                    return propertyType.GetEnumValue(ui);
                if (value is ulong ul)
                    return propertyType.GetEnumValue(ul);

                if (value is byte b)
                    return propertyType.GetEnumValue(b);
                if (value is sbyte sb)
                    return propertyType.GetEnumValue(sb);
            }

            throw new Exception("Could not resolve the value.");
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
        private static Func<object> BuildNoneGetter(Type type)
        {
            if (!IsFsharpOption(type))
                throw new ArgumentException("'type' is not FSharpOption<'T> type.");

            if (_noneGetterCache.TryGetValue(type, out Func<object> getter))
                return getter;

            var propertyInfo = type.GetProperty("None");
            getter = Expression.Lambda<Func<object>>(
                Expression.MakeMemberAccess(null, propertyInfo)
            ).Compile();
            return _noneGetterCache.GetOrAdd(type, getter);
        }
        private static ConcurrentDictionary<Type, Func<object>> _noneGetterCache = new ConcurrentDictionary<Type, Func<object>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Func<object, object> BuildSomeMethod(Type type)
        {
            if (!IsFsharpOption(type))
                throw new ArgumentException("'type' is not FSharpOption<'T> type.");

            if (_someMethodCache.TryGetValue(type, out Func<object, object> someMethod))
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
            return _someMethodCache.GetOrAdd(type, someMethod);
        }
        private static ConcurrentDictionary<Type, Func<object, object>> _someMethodCache = new ConcurrentDictionary<Type, Func<object, object>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duType"></param>
        /// <param name="valueType"></param>
        /// <returns></returns>
        private static MethodInfo GetDiscriminatedUnionsConstructorInfo(Type duType, Type valueType)
        {
            if (_duConstructorParamsCache.TryGetValue(duType, out MethodInfo info))
                return info;

            var method = duType
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m =>
                {
                    var ps = m.GetParameters();
                    return valueType == typeof(DBNull)
                        ? ps.Length == 1
                        : ps.Length == 1 && ps[0].ParameterType == valueType;
                });

            if (method == null)
                throw new ArgumentException($"There are no cases of discriminant unions that are '{valueType.FullName}' type only.");

            return _duConstructorParamsCache.GetOrAdd(duType, method);
        }
        private static ConcurrentDictionary<Type, MethodInfo> _duConstructorParamsCache = new ConcurrentDictionary<Type, MethodInfo>();
        private static ConcurrentDictionary<Type, Type> _duConstructorParamTypeCache = new ConcurrentDictionary<Type, Type>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duType"></param>
        /// <param name="valueType"></param>
        /// <returns></returns>
        private static Func<object, object> BuildDiscriminatedUnionsConstructor(Type duType, Type valueType)
        {
            if (DiscriminatedUnionsConstructorCache.TryGetSomeMethod(duType, valueType, out Func<object, object> ctor))
                return ctor;

            var method = GetDiscriminatedUnionsConstructorInfo(duType, valueType);
            valueType = method.GetParameters()[0].ParameterType;

            var value = Expression.Parameter(typeof(object), "value");
            ctor = Expression.Lambda<Func<object, object>>(
                Expression.Call(
                    null,
                    method,
                    Expression.Convert(value, method.GetParameters()[0].ParameterType)),
                value
                ).Compile();
            return DiscriminatedUnionsConstructorCache.GetOrAdd(duType, valueType, ctor);
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

        /// <summary>
        /// 
        /// </summary>
        private static class DiscriminatedUnionsConstructorCache
        {
            private static ConcurrentDictionary<Type, ConcurrentDictionary<Type, Func<object, object>>> _cache
                = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, Func<object, object>>>();

            public static Func<object, object> GetOrAdd(Type duType, Type valueType, Func<object, object> method)
            {
                if (_cache.TryGetValue(duType, out ConcurrentDictionary<Type, Func<object, object>> duCache))
                {
                    if (duCache.TryGetValue(valueType, out Func<object, object> ctor))
                        return ctor;
                    else
                        return duCache.GetOrAdd(valueType, method);
                }
                else
                {
                    return _cache.GetOrAdd(duType, new ConcurrentDictionary<Type, Func<object, object>>()).GetOrAdd(valueType, method);
                }
            }

            public static bool TryGetSomeMethod(Type duType, Type valueType, out Func<object, object> method)
            {
                method = null;
                return _cache.TryGetValue(duType, out ConcurrentDictionary<Type, Func<object, object>> cache)
                        && cache.TryGetValue(valueType, out method);
            }
        }

        private class ConstructorArgsInfo
        {
            public Type Type { get; set; }
            public string Name { get; set; }
        }

        private static bool IsFsharpOption(Type type)
        {
            return type.IsGenericType
                && !type.IsGenericTypeDefinition
                && !type.IsGenericParameter
                && typeof(FSharpOption<>) == type.GetGenericTypeDefinition();
        }

        private static bool IsNullable(Type type)
        {
            return type.IsGenericType
                && !type.IsGenericTypeDefinition
                && !type.IsGenericParameter
                && typeof(Nullable<>) == type.GetGenericTypeDefinition();
        }
    }
}
#endif