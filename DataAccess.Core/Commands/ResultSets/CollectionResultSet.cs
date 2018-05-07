using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace DataAccess
{
    public class CollectionResultSet<T> : ResultSet,
        ICollectionReader<T>
    {
        IList<T> ICollectionReader<T>.Objects { get; set; }

        public IList<T> Data => ((ICollectionReader<T>)this).Objects;

        TypeMap ICollectionReader<T>.TypeMap { get; set; }

        Action<DbDataReader, T> ITypeReader<T>.OnRecordRead { get; set; }

        PropertyMap<T> ITypeReader<T>.PropertyMap { get; set; }

        public override int Read(DbDataReader reader)
        {
            return this.ReadCollection(reader);
        }

        #region Fluent methods that cannot be intuitively extended since the compiler cannot infer types from constraints

        /// <summary>
        /// Called when the data has been read from the database reader
        /// It can be used to manually populate any objects
        /// </summary>
        /// <param name="onRecordRead"></param>
        /// <returns></returns>
        public CollectionResultSet<T> OnRecordRead(Action<DbDataReader, T> onRecordRead)
        {
            ((ITypeReader<T>)this).OnRecordRead = onRecordRead;

            return this;
        }

        /// <summary>
        /// Maps the name of the properties of the object(s) to be populated to the index of the columns returned
        /// by the database reader
        /// </summary>
        /// <param name="mappedProperties"></param>
        /// <returns></returns>
        public CollectionResultSet<T> MapProperties(params MappedProperty<T>[] mappedProperties)
        {
            ((ITypeReader<T>)this).PropertyMap = new PropertyMap<T>(mappedProperties);

            return this;
        }

        /// <summary>
        /// Maps the name of the properties of the object(s) to be populated to the index of the columns returned
        /// by the database reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="reader"></param>
        /// <param name="configures"></param>
        /// <returns></returns>
        public CollectionResultSet<T> MapProperties(params Action<MappedProperty<T>>[] configures)
        {
            return MapProperties(configures
                .Select(configure =>
                {
                    var mappedProperty = new MappedProperty<T>();

                    configure(mappedProperty);

                    return mappedProperty;
                })
                .ToArray()
            );
        }

        /// <summary>
        /// Maps the type of the item to be created to a code (number) retrieved from the query
        /// to support polymorphic queries
        /// </summary>
        /// <param name="typeDiscriminatorIndex"></param>
        /// <param name="mappedTypes"></param>
        /// <returns></returns>
        public CollectionResultSet<T> MapTypes(int typeDiscriminatorIndex, params MappedType[] mappedTypes)
        {
            ((ICollectionReader<T>)this).TypeMap = new TypeMap(typeDiscriminatorIndex, mappedTypes);

            return this;
        }

        /// <summary>
        /// Maps the type of the item to be created to a code (number) retrieved from the query
        /// to support polymorphic queries
        /// </summary>
        /// <param name="typeDiscriminatorIndex"></param>
        /// <param name="configures"></param>
        /// <returns></returns>
        public CollectionResultSet<T> MapTypes(int typeDiscriminatorIndex, params Action<MappedType>[] configures)
        {
            return MapTypes(typeDiscriminatorIndex,
                configures
                    .Select(configure =>
                    {
                        var mappedType = new MappedType();

                        configure(mappedType);

                        return (mappedType);
                    })
                    .ToArray()
            );
        }

        #endregion
    }
}