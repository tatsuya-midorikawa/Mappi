using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace Mappi
{
    public static class SqlConnectionExtensions
    {
        public static MultipleDataReader MultipleQuery(this SqlConnection connection, string sql, object parameter = null)
            => new MultipleDataReader(connection.ExecuteReader(sql, parameter));

        public static DataReader Query(this SqlConnection connection, string sql, object parameter = null)
            => new DataReader(connection.ExecuteReader(sql, parameter));

        private static SqlDataReader ExecuteReader(this SqlConnection connection, string sql, object parameter = null)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            using (var command = new SqlCommand(sql, connection))
            {
                var properties = parameter?.GetType().GetProperties() ?? new PropertyInfo[0];
                foreach (var property in properties)
                {
                    var key = $"@{property.Name}";
                    var value = property.GetValue(parameter, null);
                    command.Parameters.AddWithValue(key, value);
                }

                return command.ExecuteReader();
            }
        }
    }
}
