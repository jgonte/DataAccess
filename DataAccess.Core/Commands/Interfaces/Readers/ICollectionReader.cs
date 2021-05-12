using System.Collections.Generic;

namespace DataAccess
{
    /// <summary>
    /// Defines a type that populates a collection of strong typed records
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICollectionReader<T> : ITypeReader<T>
    {
        /// <summary>
        /// The objects read by this reader
        /// </summary>
        IList<T> Records { get; set; }
    }
}