using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    /// <summary>
    /// A database command to retrieve a single object
    /// </summary>
    public class SingleQuery<T> : Query<T>,
        IObjectReader<T>
    {
        

        public SingleQueryResponse<T> Execute(Context context = null)
        {
            ExecuteCommand(context);

            return new SingleQueryResponse<T>
            {
                ReturnCode = ReturnCode,
                Parameters = Parameters,
                Record = (T)((IObjectReader<T>)this).RecordInstance
            };
        }

        public async Task<SingleQueryResponse<T>> ExecuteAsync(Context context = null)
        {
            await ExecuteCommandAsync(context);

            return new SingleQueryResponse<T>
            {
                ReturnCode = ReturnCode,
                Parameters = Parameters,
                Record = (T)((IObjectReader<T>)this).RecordInstance
            };
        }

        public override int Read(DbDataReader reader)
        {
            return this.ReadSingle(reader);
        }

        #region Fluent methods that cannot be intuitively extended since the compiler cannot infer types from constraints

        /// <summary>
        /// Called when the data has been read from the database reader
        /// It can be used to manually populate any objects
        /// </summary>
        /// <param name="onRecordRead"></param>
        /// <returns></returns>
        public SingleQuery<T> OnRecordRead(Action<DbDataReader, T> onRecordRead)
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
        public SingleQuery<T> MapProperties(params MappedProperty[] mappedProperties)
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
        public SingleQuery<T> MapProperties(params Action<MappedProperty>[] configures)
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
        public SingleQuery<T> MapTypes(int typeDiscriminatorIndex, params MappedType[] mappedTypes)
        {
            ((ITypeReader<T>)this).TypeMap = new TypeMap(typeDiscriminatorIndex, mappedTypes);

            return this;
        }

        /// <summary>
        /// Maps the type of the item to be created to a code (number) retrieved from the query
        /// to support polymorphic queries
        /// </summary>
        /// <param name="typeDiscriminatorIndex"></param>
        /// <param name="configures"></param>
        /// <returns></returns>
        public SingleQuery<T> MapTypes(int typeDiscriminatorIndex, params Action<MappedType>[] configures)
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
