using System;

namespace DataAccess
{
    /// <summary>
    /// Exception to throw when there a violation of an optimistic concurrency
    /// </summary>
    public class DbConcurrencyException: Exception
    {
    }
}
