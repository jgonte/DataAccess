using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using Utilities;

namespace DataAccess
{
    public static class DbProviderFactories
    {
        public static DbProviderFactory GetFactory(string providerName)
        {
            switch (providerName)
            {
                case "System.Data.SqlClient": return SqlClientFactory.Instance;
                case "Microsoft.Data.Sqlite": return GetDbProviderFactory("Microsoft.Data.Sqlite.SqliteFactory", "Microsoft.Data.Sqlite");
                case "MySql.Data": return GetDbProviderFactory("MySql.Data.MySqlClient.MySqlClientFactory", "MySql.Data");
                case "Npgsql": return GetDbProviderFactory("Npgsql.NpgsqlFactory", "Npgsql");
                default: throw new NotImplementedException();
            }
        }

        public static DbProviderFactory GetDbProviderFactory(string dbProviderFactoryTypename, string assemblyName)
        {
            var instance = dbProviderFactoryTypename.ToType().GetStaticProperty("Instance");

            if (instance == null)
            {
                var a = Assembly.Load(assemblyName);

                if (a != null)
                {
                    instance = dbProviderFactoryTypename.ToType().GetStaticProperty("Instance");
                }
            }

            if (instance == null)
            {
                throw new InvalidOperationException($"Unable to retrieve DbProviderFactory from: '{dbProviderFactoryTypename}'");
            }

            return instance as DbProviderFactory;
        }
    }
}