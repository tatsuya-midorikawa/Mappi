using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Reffy;
using Microsoft.FSharp.Core;

#if NET45 || NET46 || NET472 || NET48 || NETCOREAPP3_1 || NET5_0
using System.Threading.Tasks;
#endif

namespace Mappi
{
    public sealed class DataReader : IDisposable, IDataReader
    {
        private readonly MultipleDataReader _reader;
        private bool _disposedValue;
        private bool _isRead;

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
            _isRead = false;
        }

        public IEnumerable<T> Read<T>()
        {
            if (_isRead)
                throw new Exception("The data has already been loaded.");

            _isRead = true;
            return _reader.Read<T>();
        }

#if NET45 || NET46 || NET472 || NET48 || NETCOREAPP3_1 || NET5_0
        public async Task<IEnumerable<T>> ReadAsync<T>(int baseCapacity = 128) 
            where T : class
        {
            if (_isRead)
                throw new Exception("The data has already been loaded.");

            _isRead = true;
            return await _reader.ReadAsync<T>();
        }
#endif
    }
}
