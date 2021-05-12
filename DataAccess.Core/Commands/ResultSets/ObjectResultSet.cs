using System;
using System.Data.Common;
using System.Linq;

namespace DataAccess
{
    public class ObjectResultSet<T> : ResultSet,
        IObjectReader<T>
    {
        T IObjectReader<T>.Record { get; set; }

        public T Data => ((IObjectReader<T>)this).Record;

        Action<DbDataReader, T> ITypeReader<T>.OnRecordRead { get; set; }

        PropertyMap ITypeReader<T>.PropertyMap { get; set; }

        TypeMap ITypeReader<T>.TypeMap { get; set; }

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
        public ObjectResultSet<T> OnRecordRead(Action<DbDataReader, T> onRecordRead)
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
        public ObjectResultSet<T> MapProperties(params MappedProperty[] mappedProperties)
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
        public ObjectResultSet<T> MapProperties(params Action<MappedProperty>[] configures)
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

        #endregion
    }
}