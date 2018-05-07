using System.Data.Common;

namespace DataAccess
{
    /// <summary>
    /// The item of a multiple results command that reads its own object
    /// </summary>
    public abstract class ResultSet : IReader
    {
        public abstract int Read(DbDataReader reader);

        /// <summary>
        /// A result set that containsa single object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ObjectResultSet<T> Object<T>() => new ObjectResultSet<T>();

        /// <summary>
        /// A result set that contains a collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static CollectionResultSet<T> Collection<T>() => new CollectionResultSet<T>();
    }
}