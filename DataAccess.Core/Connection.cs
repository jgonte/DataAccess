namespace DataAccess
{
    /// <summary>
    /// Describes a database connection
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// The name of the connection
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The name of the provider
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// The connection string
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
