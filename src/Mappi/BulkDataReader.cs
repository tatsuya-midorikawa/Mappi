using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Reffy;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Reflection;
using Mono.Reflection;
using System.Data;

namespace Mappi
{
    public struct BulkDataReader : IDisposable
    {
        private DataTable _table;
        private bool _disposedValue;

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _table?.Dispose();
                }

                _table = null;
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public BulkDataReader(DataTable table)
        {
            _table = table;
            _disposedValue = false;
        }

        public int RowCount
            => _table.Rows.Count;

        public IEnumerable<T> EnumerateRows<T>()
        {
            var type = typeof(T);
            foreach (DataRow row in _table.Rows)
            {
                var instance = type.MakeDefault();

                foreach (var property in type
                       .GetProperties((BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ^ BindingFlags.DeclaredOnly)
                       .Where(property => property.GetAttribute<IgnoreAttribute>() == null))
                {
                    var columnName = property.GetAttribute<ColumnAttribute>() is ColumnAttribute c ? c.Name : property.Name;
                    var value = Resolve(property.PropertyType, row[columnName]);
                    property.GetBackingField().SetValueDirect(__makeref(instance), value);
                }

                yield return (T)instance;
            }
        }

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
