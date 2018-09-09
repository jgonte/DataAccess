using System;
using System.Data.Common;

namespace DataAccess
{
    /// <summary>
    /// Defines a type that populates a strong type 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITypeReader<T> : IReader
    {
        /// <summary>
        /// Called when a record is read from a database reader
        /// </summary>
        Action<DbDataReader, T> OnRecordRead { get; set; }

        /// <summary>
        /// The map of the properties to the index of the columns of the reader
        /// </summary>
        PropertyMap PropertyMap { get; set; }

        /// <summary>
        /// The type map to support polymorphic queries by mapping the type of the item to be created
        /// to a code (number) retrieved from the query
        /// </summary>
        TypeMap TypeMap { get; set; }
    }
}