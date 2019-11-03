using System.Data.Common;

namespace DataAccess
{
    /// <summary>
    /// Deals with the database vendor differences
    /// </summary>
    public abstract class DatabaseDriver
    {
        /// <summary>
        /// Retrieves the parameter placeholder of the database
        /// </summary>
        public abstract string ParameterPlaceHolder { get; }

        /// <summary>
        /// Sets custom parameter types not supported by the generic ones
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="type"></param>
        public abstract void SetParameterType(DbParameter parameter, int type);
    }
}
