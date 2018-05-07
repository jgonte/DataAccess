using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Utilities;

namespace DataAccess
{
    public static class DataAccessExtensions
    {
        /// <summary>
        /// Creates a database command from a connection
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandType"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="timeout"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        public static DbCommand CreateCommand(
            this DbConnection connection,
            CommandType commandType,
            string sql,
            IList<Parameter> parameters,
            int? timeout,
            DatabaseDriver driver)
        {
            var command = connection.CreateCommand();

            command.CommandType = commandType;

            if (timeout.HasValue)
            {
                command.CommandTimeout = timeout.Value;
            }

            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentNullException("Must provide the SQL statement");
            }

            command.CommandText = sql;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    command.AddParameter(
                        driver.ParameterPlaceHolder + parameter._name,
                        parameter._value,
                        parameter._size,
                        parameter._direction,
                        parameter._type,
                        driver);
                }
            }

            return command;
        }

        /// <summary>
        /// Adds a parameter to the command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="size"></param>
        /// <param name="direction"></param>
        /// <param name="sqlType"></param>
        /// <param name="driver"></param>
        public static void AddParameter(
            this DbCommand command, 
            string name, 
            object value, 
            int? size, 
            ParameterDirection direction, 
            int? sqlType, 
            DatabaseDriver driver)
        {
            var parameter = command.CreateParameter();

            parameter.ParameterName = name;

            parameter.Value = (value == null) ? DBNull.Value : value;

            if (size.HasValue)
            {
                parameter.Size = size.Value;
            }

            parameter.Direction = direction;

            if (sqlType.HasValue)
            {
                driver.SetParameterType(parameter, sqlType.Value);
            }

            command.Parameters.Add(parameter);
        }

        #region Reader extensions

        public static string GetStringOrNull(this DbDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : reader.GetString(i);
        }

        public static char GetCharacter(this DbDataReader reader, int i)
        {
            try
            {
                return reader.GetChar(i); // GetChar is not supported for System.Data.SqlClient
            }
            catch
            {
                return reader.GetString(i)[0];
            }
        }

        public static char? GetCharacterOrNull(this DbDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : reader.GetCharacter(i).ToNullable();
        }

        public static bool? GetBooleanOrNull(this DbDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : reader.GetBoolean(i).ToNullable();
        }

        public static byte? GetByteOrNull(this DbDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : reader.GetByte(i).ToNullable();
        }

        public static sbyte? GetSignedByteOrNull(this DbDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : ((sbyte)reader.GetByte(i)).ToNullable();
        }

        public static short? GetInt16OrNull(this DbDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : reader.GetInt16(i).ToNullable();
        }

        public static ushort? GetUnsignedInt16OrNull(this DbDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : ((ushort)reader.GetInt16(i)).ToNullable();
        }

        public static int? GetInt32OrNull(this DbDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : reader.GetInt32(i).ToNullable();
        }

        public static uint? GetUnsignedInt32OrNull(this DbDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : ((uint)reader.GetInt32(i)).ToNullable();
        }

        public static long? GetInt64OrNull(this DbDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : reader.GetInt64(i).ToNullable();
        }

        public static ulong? GetUnsignedInt64OrNull(this DbDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : ((ulong)reader.GetInt64(i)).ToNullable();
        }

        public static float? GetFloatOrNull(this DbDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : reader.GetFloat(i).ToNullable();
        }

        public static double? GetDoubleOrNull(this DbDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : reader.GetDouble(i).ToNullable();
        }

        public static decimal? GetDecimalOrNull(this DbDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : reader.GetDecimal(i).ToNullable();
        }

        public static Guid? GetGuidOrNull(this DbDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : reader.GetGuid(i).ToNullable();
        }

        public static DateTime? GetDateTimeOrNull(this DbDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : reader.GetDateTime(i).ToNullable();
        }

        public static object GetValueOrNull(this DbDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : reader.GetValue(i);
        } 

        #endregion
    }
}
