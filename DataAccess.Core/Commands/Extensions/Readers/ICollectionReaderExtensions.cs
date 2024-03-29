﻿using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Utilities;

namespace DataAccess
{
    public static class ICollectionReaderExtensions
    {
        /// <summary>
        /// The instance of the object to populate from the read record
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static T RecordInstances<T, U>(this T reader, IList<U> items)
            where T : ICollectionReader<U>
        {
            reader.RecordInstances = items;

            return reader;
        }

        internal static int ReadCollection<T>(this ICollectionReader<T> reader, DbDataReader dbReader)
        {
            if (reader.RecordInstances == null)
            {
                reader.RecordInstances = new List<T>();
            }

            if (dbReader.HasRows)
            {
                var i = 0; // Index to support indexing existing collections

                while (dbReader.Read())
                {
                    var obj = ReadObject(reader, dbReader, i++);

                    if (reader.RecordInstances.ElementAtOrDefault(i) == null)
                    {
                        reader.RecordInstances.Add(obj); // Add the object to the list if it not already exists
                    }
                }

                return reader.RecordInstances.Count;
            }

            return 0;
        }

        private static T ReadObject<T>(ICollectionReader<T> reader, DbDataReader dbReader, int index)
        {
            // Retrieve an existin item or create a new one
            T obj = default(T);

            if (reader.RecordInstances.ElementAtOrDefault(index) != null) // Use an existing item if any
            {
                obj = reader.RecordInstances.ElementAt(index);
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
