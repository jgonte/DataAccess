using System.Text.RegularExpressions;
using System.Data;
using System.Threading.Tasks;

namespace DataAccess
{
    /// <summary>
    /// Executes a database script
    /// </summary>
    public static class ScriptExecutor
    {
        /// <summary>
        /// Executes a database script
        /// </summary>
        /// <param name="connection">The information of the connection to the database</param>
        /// <param name="script">The script to be executed</param>
        /// <param name="batchDelimiter">The regex batch delimiter: "^GO" for SQL Server</param>
        public static void ExecuteScript(Connection connection, string script, string batchDelimiter)
        {
            if (batchDelimiter == null) // Execute the whole script
            {
                new NonQueryCommand
                {
                    _connection = connection,
                    _type = CommandType.Text,
                    _text = script,
                    DatabaseDriver = DatabaseDriverManager.Drivers[connection.ProviderName]
                }
                .Execute(null);
            }
            else
            {
                Regex regex = new Regex(batchDelimiter, RegexOptions.IgnoreCase | RegexOptions.Multiline);

                foreach (string sql in regex.Split(script))
                {
                    if (string.IsNullOrWhiteSpace(sql))
                    {
                        continue;
                    }

                    new NonQueryCommand
                    {
                        _connection = connection,
                        _type = CommandType.Text,
                        _text = sql,
                        DatabaseDriver = DatabaseDriverManager.Drivers[connection.ProviderName]
                    }
                    .Execute(null);
                }
            }
        }

        /// <summary>
        /// Executes a database script
        /// </summary>
        /// <param name="connection">The information of the connection to the database</param>
        /// <param name="script">The script to be executed</param>
        /// <param name="batchDelimiter">The regex batch delimiter: "^GO" for SQL Server</param>
        public static async Task ExecuteScriptAsync(Connection connection, string script, string batchDelimiter)
        {
            if (batchDelimiter == null) // Execute the whole script
            {
                await new NonQueryCommand
                {
                    _connection = connection,
                    _type = CommandType.Text,
                    _text = script,
                    DatabaseDriver = DatabaseDriverManager.Drivers[connection.ProviderName]
                }
                .ExecuteAsync(null);
            }
            else
            {
                Regex regex = new Regex(batchDelimiter, RegexOptions.IgnoreCase | RegexOptions.Multiline);

                foreach (string sql in regex.Split(script))
                {
                    if (string.IsNullOrWhiteSpace(sql))
                    {
                        continue;
                    }

                    await new NonQueryCommand
                    {
                        _connection = connection,
                        _type = CommandType.Text,
                        _text = sql,
                        DatabaseDriver = DatabaseDriverManager.Drivers[connection.ProviderName]
                    }
                    .ExecuteAsync(null);
                }
            }
        }
    }
}
