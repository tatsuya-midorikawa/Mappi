using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Reffy;
using Microsoft.FSharp.Core;

namespace Mappi
{
    public struct MultipleDataReader : IDisposable, IDataReader
    {
        private SqlDataReader _reader;
        private bool _disposedValue;

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

        public MultipleDataReader(SqlDataReader reader)
        {
            _reader = reader;
            _disposedValue = false;
            HasNext = true;
        }

        public bool HasNext { get; private set; }

        public IEnumerable<T> Read<T>()
            where T : new()
        {
            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => !field.Name.EndsWith("k__BackingField"))
                .ToArray();
            
            while (_reader.Read())
            {
                var instance = new T();
                var refinst = __makeref(instance);
                foreach (var property in properties)
                {
                    // プロパティにIgnore属性が付与されている場合はマッピング対象外にする
                    if (property.GetAttribute<IgnoreAttribute>() != null)
                        continue;

                    // rowから値を取得
                    var value = (property.GetAttribute<ColumnAttribute>() is ColumnAttribute column)
                        ? _reader[column.Name]
                        : _reader[property.Name];

                    // プロパティのBackingFieldへセットする値を生成
                    var backingfield = property.Name.GetBackingField<T>();
                    if (backingfield == null)
                        continue;
                    var input = Map(value, backingfield);

                    // T型が構造体の場合、nullを突っ込もうとすると例外が発生するのでちゃんと値があるか見切る
                    if (type.IsClass || input != null)
                        backingfield.SetValueDirect(refinst, input);
                }

                foreach (var field in fields)
                {
                    // フィールドにIgnore属性が付与されている場合はマッピング対象外にする
                    if (field.GetAttribute<IgnoreAttribute>() != null)
                        continue;

                    // rowから値を取得
                    var value = (field.GetAttribute<ColumnAttribute>() is ColumnAttribute column)
                        ? _reader[column.Name]
                        : _reader[field.Name];

                    // フィールドへセットする値を生成
                    var input = Map(value, field);

                    // T型が構造体の場合、nullを突っ込もうとすると例外が発生するのでちゃんと値があるか見切る
                    if (type.IsClass || input != null)
                        field.SetValueDirect(refinst, input);
                }

                yield return instance;
            }

            HasNext = _reader.NextResult();
        }

        private object Map(object value, FieldInfo field)
        {
            var type = field.FieldType;

            // nullable values
            if (type == typeof(Guid?)
                || type == typeof(sbyte?)
                || type == typeof(byte?)
                || type == typeof(char?)
                || type == typeof(bool?)
                || type == typeof(short?)
                || type == typeof(int?)
                || type == typeof(long?)
                || type == typeof(float?)
                || type == typeof(double?)
                || type == typeof(decimal?)
                || type == typeof(string)
                || type == typeof(DateTime?)
                || type == typeof(DateTimeOffset?)
                || type == typeof(TimeSpan?)
                || type == typeof(char[])
                || type == typeof(byte[]))
                return (value is DBNull) ? null : value;

            // non nullable values
            if (type == typeof(Guid))
                return (value is DBNull) ? Guid.Empty : value;
            if (type == typeof(sbyte))
                return (value is DBNull) ? default(sbyte) : value;
            if (type == typeof(byte))
                return (value is DBNull) ? default(byte) : value;
            if (type == typeof(char))
                return (value is DBNull) ? default(char) : value;
            if (type == typeof(bool))
                return (value is DBNull) ? default(bool) : value;
            if (type == typeof(short))
                return (value is DBNull) ? default(short) : value;
            if (type == typeof(int))
                return (value is DBNull) ? default(int) : value;
            if (type == typeof(long))
                return (value is DBNull) ? default(long) : value;
            if (type == typeof(float))
                return (value is DBNull) ? default(float) : value;
            if (type == typeof(double))
                return (value is DBNull) ? default(double) : value;
            if (type == typeof(decimal))
                return (value is DBNull) ? default(decimal) : value;
            if (type == typeof(DateTime))
                return (value is DBNull) ? default(DateTime) : value;
            if (type == typeof(DateTimeOffset))
                return (value is DBNull) ? default(DateTimeOffset) : value;
            if (type == typeof(TimeSpan))
                return (value is DBNull) ? default(TimeSpan) : value;

            // optional values
            if (type == typeof(FSharpOption<Guid>))
                return (value is DBNull) ? FSharpOption<Guid>.None : FSharpOption<Guid>.Some((Guid)value);
            if (type == typeof(FSharpOption<sbyte>))
                return (value is DBNull) ? FSharpOption<sbyte>.None : FSharpOption<sbyte>.Some(Convert.ToSByte(value));
            if (type == typeof(FSharpOption<byte>))
                return (value is DBNull) ? FSharpOption<byte>.None : FSharpOption<byte>.Some(Convert.ToByte(value));
            if (type == typeof(FSharpOption<char>))
                return (value is DBNull) ? FSharpOption<char>.None : FSharpOption<char>.Some(Convert.ToChar(value));
            if (type == typeof(FSharpOption<bool>))
                return (value is DBNull) ? FSharpOption<bool>.None : FSharpOption<bool>.Some(Convert.ToBoolean(value));
            if (type == typeof(FSharpOption<short>))
                return (value is DBNull) ? FSharpOption<short>.None : FSharpOption<short>.Some(Convert.ToInt16(value));
            if (type == typeof(FSharpOption<int>))
                return (value is DBNull) ? FSharpOption<int>.None : FSharpOption<int>.Some(Convert.ToInt32(value));
            if (type == typeof(FSharpOption<long>))
                return (value is DBNull) ? FSharpOption<long>.None : FSharpOption<long>.Some(Convert.ToInt64(value));
            if (type == typeof(FSharpOption<float>))
                return (value is DBNull) ? FSharpOption<float>.None : FSharpOption<float>.Some(Convert.ToSingle(value));
            if (type == typeof(FSharpOption<double>))
                return (value is DBNull) ? FSharpOption<double>.None : FSharpOption<double>.Some(Convert.ToDouble(value));
            if (type == typeof(FSharpOption<decimal>))
                return (value is DBNull) ? FSharpOption<decimal>.None : FSharpOption<decimal>.Some(Convert.ToDecimal(value));
            if (type == typeof(FSharpOption<DateTime>))
                return (value is DBNull) ? FSharpOption<DateTime>.None : FSharpOption<DateTime>.Some((DateTime)value);
            if (type == typeof(FSharpOption<DateTimeOffset>))
                return (value is DBNull) ? FSharpOption<DateTimeOffset>.None : FSharpOption<DateTimeOffset>.Some((DateTimeOffset)value);
            if (type == typeof(FSharpOption<TimeSpan>))
                return (value is DBNull) ? FSharpOption<TimeSpan>.None : FSharpOption<TimeSpan>.Some((TimeSpan)value);

            return (value is DBNull) ? default : value;
        }
    }
}
