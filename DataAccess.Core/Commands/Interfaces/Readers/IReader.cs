using System.Data.Common;

namespace DataAccess
{
    /// <summary>
    /// Defines a type that reads from a database reader
    /// </summary>
    public interface IReader
    {
        int Read(DbDataReader reader);
    }
}
