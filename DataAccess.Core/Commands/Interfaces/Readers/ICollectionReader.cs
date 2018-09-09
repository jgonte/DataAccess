using System.Collections.Generic;

namespace DataAccess
{
    /// <summary>
    /// Defines a type that populates a collection of strong typed items
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICollectionReader<T> : ITypeReader<T>
    {
        /// <summary>
        /// The objects populated by this reader
        /// </summary>
        IList<T> Objects { get; set; }
    }
}