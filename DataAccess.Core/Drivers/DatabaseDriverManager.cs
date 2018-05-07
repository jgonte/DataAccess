using System.Collections.Generic;

namespace DataAccess
{
    public static class DatabaseDriverManager
    {
        /// <summary>
        /// The registered drivers with the driver manager
        /// </summary>
        public static IDictionary<string, DatabaseDriver> Drivers { get; set; }

        static DatabaseDriverManager()
        {
            Drivers = new Dictionary<string, DatabaseDriver>();

            Drivers.Add("System.Data.SqlClient", new SqlServerDatabaseDriver());

            LoadDrivers();
        }

        private static void LoadDrivers()
        {
            string driversDirectory = "";
            //throw new System.NotImplementedException();
        }
    }
}
