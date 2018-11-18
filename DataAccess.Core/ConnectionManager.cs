using Microsoft.Extensions.Configuration;
using NetCoreHelpers;
using System.Collections.Generic;
using System.Linq;

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

            var connections = ConfigurationHelper.GetConfiguration().GetSection("Connections");

            if (!connections.Exists())
            {
                throw new System.InvalidOperationException("No connections were configured");
            }

            foreach (var connection in connections.GetChildren())
            {
                Connections.Add(connection.Key, new Connection
                {
                    Name = connection.Key,
                    ProviderName = connection.GetChildren().Single(ch => ch.Key == "ProviderName").Value,
                    ConnectionString = connection.GetChildren().Single(ch => ch.Key == "ConnectionString").Value
                });
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
