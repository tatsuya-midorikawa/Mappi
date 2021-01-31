using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using Reffy;

namespace Mappi
{
    public struct DataReader : IDisposable
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

        public DataReader(SqlDataReader reader)
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
            var properties = type.GetProperties();

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

                    // プロパティのBackingFieldへ値を設定
                    var backingfield = property.Name.GetBackingField<T>();
                    var input = Map(value, backingfield);

                    // T型が構造体の場合、nullを突っ込もうとすると例外が発生するのでちゃんと値があるか見切る
                    if (type.IsClass || input != null)
                        backingfield.SetValueDirect(refinst, input);
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

            return (value is DBNull) ? default : value;
        }
    }
}
