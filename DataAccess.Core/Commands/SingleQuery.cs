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
        T IObjectReader<T>.Object { get; set; }

        public T Data => ((IObjectReader<T>)this).Object;

        public Response<T> Execute(Context context = null)
        {
            ExecuteCommand(context);

            return new Response<T>
            {
                ReturnCode = ReturnCode,

                Data = Data
            };
        }

        public async Task<Response<T>> ExecuteAsync(Context context = null)
        {
            await ExecuteCommandAsync(context);

            return new Response<T>
            {
                ReturnCode = ReturnCode,

                Data = Data
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
        public SingleQuery<T> MapProperties(params MappedProperty<T>[] mappedProperties)
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
        public SingleQuery<T> MapProperties(params Action<MappedProperty<T>>[] configures)
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

        #endregion
    }
}
