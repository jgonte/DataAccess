using System.Data.Common;

namespace DataAccess
{
    /// <summary>
    /// The context where the database command executes
    /// </summary>
    public class Context
    {
        /// <summary>
        /// The connection to be used
        /// </summary>
        public DbConnection Connection { get; set; }

        /// <summary>
        /// The transaction to be used
        /// </summary>
        public DbTransaction Transaction { get; set; }
    }
}
