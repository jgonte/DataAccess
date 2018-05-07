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

        /// <summary>
        /// The type map to support polymorphic queries by mapping the type of the item to be created
        /// to a code (number) retrieved from the query
        /// </summary>
        TypeMap TypeMap { get; set; }
    }
}