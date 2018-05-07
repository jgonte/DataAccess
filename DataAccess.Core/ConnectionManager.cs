using System.Collections.Generic;
using System.Configuration;

namespace DataAccess
{
    /// <summary>
    /// Singleton with the database connections
    /// </summary>
    public static class ConnectionManager
    {
        static ConnectionManager()
        {
            // Load all the connections from the configuration
            Connections = new Dictionary<string, Connection>();

            foreach (ConnectionStringSettings connectionSettings in ConfigurationManager.ConnectionStrings)
            {
                var connection = new Connection
                {
                    Name = connectionSettings.Name,
                    ProviderName = connectionSettings.ProviderName,
                    ConnectionString = connectionSettings.ConnectionString
                };

                Connections.Add(connection.Name, connection);
            }
        }

        /// <summary>
        /// Retrieves the database connection
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Connection GetConnection(string name)
        {
            return Connections[name];
        }

        /// <summary>
        /// The database connections
        /// </summary>
        public static IDictionary<string, Connection> Connections { get; private set; }
    }
}
