using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Reffy;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Reflection;
using System.Data;

namespace Mappi
{
    public struct MultipleBulkDataReader : IDisposable
    {
        private readonly DataSet _dataset;
        private readonly BulkDataReader[] _tables;
        private bool _disposedValue;

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var table in _tables)
                        table.Dispose();
                    _dataset?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public MultipleBulkDataReader(DataSet dataset)
        {
            _dataset = dataset;
            _tables = dataset.Tables.Cast<DataTable>().Select(table => new BulkDataReader(table)).ToArray();
            _disposedValue = false;
        }

        public BulkDataReader[] Tables => _tables;
    }
}
