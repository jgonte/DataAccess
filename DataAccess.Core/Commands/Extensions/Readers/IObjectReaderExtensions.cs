using System;
using System.Data.Common;
using Utilities;

namespace DataAccess
{
    public static class IObjectReaderExtensions
    {
        /// <summary>
        /// The instance of the object to populate from the read record
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T Instance<T, U>(this T or, U obj)
            where T : IObjectReader<U>
        {
            or.Object = obj;

            return or;
        }

        /// <summary>
        /// Called when the data has been read from the database reader
        /// It can be used to manually populate any objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="reader"></param>
        /// <param name="onRecordRead"></param>
        /// <returns></returns>
        public static T OnRecordRead<T, U>(this T reader, Action<DbDataReader, U> onRecordRead)
            where T : IObjectReader<U>
        {
            reader.OnRecordRead = onRecordRead;

            return reader;
        }

        internal static int ReadSingle<T>(this IObjectReader<T> reader, DbDataReader dbReader)
        {
            if (dbReader.Read())
            {
                reader.Object = ReadObject(reader, dbReader);
            }

            if (dbReader.Read())
            {
                throw new InvalidOperationException("Query returned more than one record");
            }

            return reader.Object == null ? 0 : 1;
        }

        private static T ReadObject<T>(IObjectReader<T> reader, DbDataReader dbReader)
        {
            // Retrieve an existin item or create a new one
            T obj = default(T);

            if (reader.Object != null) // Use an existing instance if any
            {
                obj = reader.Object;
            }
            else if (reader.TypeMap != null)
            {
                obj = (T)reader.TypeMap.CreateObject(dbReader);
            }
            else
            {
                obj = (T)typeof(T).CreateInstance();
            }
            
            reader.ReadRecord(dbReader, obj); // Populate the item from the reader

            return obj;
        }
    }
}
