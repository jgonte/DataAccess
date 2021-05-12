using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    /// <summary>
    /// A database command that populates a collection of objects from a database reader
    /// </summary>
    public class CollectionQuery<T> : Query<T>,
        ICollectionReader<T>
    {
        TypeMap ITypeReader<T>.TypeMap { get; set; }

        IList<T> ICollectionReader<T>.Records { get; set; }

        public override int Read(DbDataReader reader)
        {
            return this.ReadCollection(reader);
        }

        public CollectionQueryResponse<T> Execute(Context context = null)
        {
            ExecuteCommand(context);

            return CreateResponse();
        }

        public async Task<CollectionQueryResponse<T>> ExecuteAsync(Context context = null)
        {
            await ExecuteCommandAsync(context);

            return CreateResponse();
        }

        private CollectionQueryResponse<T> CreateResponse()
        {
            return new CollectionQueryResponse<T>
            {
                ReturnCode = ReturnCode,
                Parameters = Parameters,
                Records = ((ICollectionReader<T>)this).Records,
                Count = GetCount(Parameters, ((ICollectionReader<T>)this).Records)
            };
        }

        private int GetCount(List<Parameter> parameters, IList<T> records)
        {
            var countParameter = parameters.Where(p => p.Name == "count").SingleOrDefault();

            return countParameter != null ? (int)countParameter.Value : records.Count;
        }

        #region Fluent methods that cannot be intuitively extended since the compiler cannot infer types from constraints

        /// <summary>
        /// Called when the data has been read from the database reader
        /// It can be used to manually populate any objects
        /// </summary>
        /// <param name="onRecordRead"></param>
        /// <returns></returns>
        public CollectionQuery<T> OnRecordRead(Action<DbDataReader, T> onRecordRead)
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
        public CollectionQuery<T> MapProperties(params MappedProperty[] mappedProperties)
        {
            ((ITypeReader<T>)this).PropertyMap = new PropertyMap(mappedProperties);

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
        public CollectionQuery<T> MapProperties(params Action<MappedProperty>[] configures)
        {
            return MapProperties(configures
                .Select(configure =>
                {
                    var mappedProperty = new MappedProperty();

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
        public CollectionQuery<T> MapTypes(int typeDiscriminatorIndex, params MappedType[] mappedTypes)
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
        public CollectionQuery<T> MapTypes(int typeDiscriminatorIndex, params Action<MappedType>[] configures)
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
