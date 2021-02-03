using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace Mappi
{
    public static class SqlConnectionExtensions
    {
        public static MultipleDataReader MultipleQuery(this SqlConnection connection, string sql, object parameter = null)
            => new MultipleDataReader(connection.ExecuteReader(sql, parameter));

        public static DataReader Query(this SqlConnection connection, string sql, object parameter = null)
            => new DataReader(connection.ExecuteReader(sql, parameter));

#if NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1 || NET5_0
        private static InvalidOperationException _invalidOperationException 
            = new InvalidOperationException("There is a missing SELECT expression.");

        public static (T1[], T2[]) MultipleQuery<T1, T2>(this SqlConnection connection, string sql, object parameter = null)
            where T1 : new()
            where T2 : new()
        {
            using (var reader = connection.MultipleQuery(sql, parameter))
            {
                var result1 = reader.Read<T1>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result2 = reader.Read<T2>().ToArray();

                return (result1, result2);
            }
        }

        public static (T1[], T2[], T3[]) MultipleQuery<T1, T2, T3>(this SqlConnection connection, string sql, object parameter = null)
            where T1 : new()
            where T2 : new()
            where T3 : new()
        {
            using (var reader = connection.MultipleQuery(sql, parameter))
            {
                var result1 = reader.Read<T1>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result2 = reader.Read<T2>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result3 = reader.Read<T3>().ToArray();
                return (result1, result2, result3);
            }
        }

        public static (T1[], T2[], T3[], T4[]) MultipleQuery<T1, T2, T3, T4>(this SqlConnection connection, string sql, object parameter = null)
            where T1 : new()
            where T2 : new()
            where T3 : new()
            where T4 : new()
        {
            using (var reader = connection.MultipleQuery(sql, parameter))
            {
                var result1 = reader.Read<T1>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result2 = reader.Read<T2>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result3 = reader.Read<T3>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result4 = reader.Read<T4>().ToArray();

                return (result1, result2, result3, result4);
            }
        }

        public static (T1[], T2[], T3[], T4[], T5[]) MultipleQuery<T1, T2, T3, T4, T5>(this SqlConnection connection, string sql, object parameter = null)
            where T1 : new()
            where T2 : new()
            where T3 : new()
            where T4 : new()
            where T5 : new()
        {
            using (var reader = connection.MultipleQuery(sql, parameter))
            {
                var result1 = reader.Read<T1>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result2 = reader.Read<T2>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result3 = reader.Read<T3>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result4 = reader.Read<T4>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result5 = reader.Read<T5>().ToArray();

                return (result1, result2, result3, result4, result5);
            }
        }

        public static (T1[], T2[], T3[], T4[], T5[], T6[]) MultipleQuery<T1, T2, T3, T4, T5, T6>(this SqlConnection connection, string sql, object parameter = null)
            where T1 : new()
            where T2 : new()
            where T3 : new()
            where T4 : new()
            where T5 : new()
            where T6 : new()
        {
            using (var reader = connection.MultipleQuery(sql, parameter))
            {
                var result1 = reader.Read<T1>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result2 = reader.Read<T2>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result3 = reader.Read<T3>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result4 = reader.Read<T4>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result5 = reader.Read<T5>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result6 = reader.Read<T6>().ToArray();

                return (result1, result2, result3, result4, result5, result6);
            }
        }

        public static (T1[], T2[], T3[], T4[], T5[], T6[], T7[]) MultipleQuery<T1, T2, T3, T4, T5, T6, T7>(this SqlConnection connection, string sql, object parameter = null)
            where T1 : new()
            where T2 : new()
            where T3 : new()
            where T4 : new()
            where T5 : new()
            where T6 : new()
            where T7 : new()
        {
            using (var reader = connection.MultipleQuery(sql, parameter))
            {
                var result1 = reader.Read<T1>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result2 = reader.Read<T2>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result3 = reader.Read<T3>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result4 = reader.Read<T4>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result5 = reader.Read<T5>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result6 = reader.Read<T6>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result7 = reader.Read<T7>().ToArray();

                return (result1, result2, result3, result4, result5, result6, result7);
            }
        }

        public static (T1[], T2[], T3[], T4[], T5[], T6[], T7[], T8[]) MultipleQuery<T1, T2, T3, T4, T5, T6, T7, T8>(this SqlConnection connection, string sql, object parameter = null)
            where T1 : new()
            where T2 : new()
            where T3 : new()
            where T4 : new()
            where T5 : new()
            where T6 : new()
            where T7 : new()
            where T8 : new()
        {
            using (var reader = connection.MultipleQuery(sql, parameter))
            {
                var result1 = reader.Read<T1>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result2 = reader.Read<T2>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result3 = reader.Read<T3>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result4 = reader.Read<T4>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result5 = reader.Read<T5>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result6 = reader.Read<T6>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result7 = reader.Read<T7>().ToArray();
                if (!reader.HasNext)
                    throw _invalidOperationException;

                var result8 = reader.Read<T8>().ToArray();

                return (result1, result2, result3, result4, result5, result6, result7, result8);
            }
        }
#endif


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
