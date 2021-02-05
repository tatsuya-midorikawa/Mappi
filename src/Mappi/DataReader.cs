using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Reffy;
using Microsoft.FSharp.Core;

namespace Mappi
{
    public struct DataReader : IDisposable, IDataReader
    {
        private MultipleDataReader _reader;
        private bool _disposedValue;

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _reader.Dispose();
                }

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
            _reader = new MultipleDataReader(reader);
            _disposedValue = false;
        }

        public IEnumerable<T> Read<T>()
            => _reader.Read<T>();
    }
}
