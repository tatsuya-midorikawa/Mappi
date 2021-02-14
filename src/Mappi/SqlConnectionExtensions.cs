using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

#if NET45 || NET46 || NET472 || NET48 || NETCOREAPP3_1 || NET5_0
using System.Threading.Tasks;
#endif

namespace Mappi
{
    public static class SqlConnectionExtensions
    {
        public static MultipleDataReader MultipleQuery(this SqlConnection connection, string sql, object parameter = null)
            => new MultipleDataReader(connection.ExecuteReader(sql, parameter));

        public static DataReader Query(this SqlConnection connection, string sql, object parameter = null)
            => new DataReader(connection.ExecuteReader(sql, parameter));

        public static MultipleBulkDataReader MultipleBulkQuery(this SqlConnection connection, string sql, object parameter = null)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            using (var command = new SqlCommand(sql, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                foreach (var p in MakeParameters(parameter))
                    command.Parameters.AddWithValue(p.Key, p.Value);

                var ds = new DataSet();
                adapter.Fill(ds);

                return new MultipleBulkDataReader(ds);
            }
        }

        public static BulkDataReader BulkQuery(this SqlConnection connection, string sql, object parameter = null)
            => connection.MultipleBulkQuery(sql, parameter).Tables.FirstOrDefault();

#if NET45 || NET46 || NET472 || NET48 || NETCOREAPP3_1 || NET5_0
        public static async Task<MultipleDataReader> MultipleQueryAsync(this SqlConnection connection, string sql, object parameter = null)
            => new MultipleDataReader(await connection.ExecuteReaderAsync(sql, parameter));

        public static async Task<DataReader> QueryAsync(this SqlConnection connection, string sql, object parameter = null)
            => new DataReader(await connection.ExecuteReaderAsync(sql, parameter));

        private static Task<SqlDataReader> ExecuteReaderAsync(this SqlConnection connection, string sql, object parameter = null)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            using (var command = new SqlCommand(sql, connection))
            {
                var properties = parameter?.GetType().GetProperties() ?? new PropertyInfo[0];
                foreach (var p in MakeParameters(parameter))
                    command.Parameters.AddWithValue(p.Key, p.Value);
                return Task<SqlDataReader>.Factory.FromAsync(
                    command.BeginExecuteReader(),
                    command.EndExecuteReader
                    );
            }
        }
#endif

        private static SqlDataReader ExecuteReader(this SqlConnection connection, string sql, object parameter = null)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            using (var command = new SqlCommand(sql, connection))
            {
                foreach (var p in MakeParameters(parameter))
                    command.Parameters.AddWithValue(p.Key, p.Value);
                return command.ExecuteReader();
            }
        }

        private static IEnumerable<KeyValuePair<string, object>> MakeParameters(object parameter)
        {
            if (parameter is null)
                return new KeyValuePair<string, object>[0];

            var properties = parameter?.GetType().GetProperties() ?? new PropertyInfo[0];
            return properties.Select(property => new KeyValuePair<string, object>($"@{property.Name}", property.GetValue(parameter, null)));
        }
    }
}
