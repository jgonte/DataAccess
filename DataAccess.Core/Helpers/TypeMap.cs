using System;
using System.Collections.Generic;
using System.Data.Common;
using Utilities;

namespace DataAccess
{
    /// <summary>
    /// Maps the value returned by the reader in the type discriminator index column to a type
    /// </summary>
    public class TypeMap
    {
        private int _typeDiscriminatorIndex;

        private IDictionary<int, Type> _map = new Dictionary<int, Type>();

        public TypeMap(int typeDiscriminatorIndex, params MappedType[] mappedTypes)
        {
            _typeDiscriminatorIndex = typeDiscriminatorIndex;

            var i = 1;

            foreach (var mappedType in mappedTypes)
            {
                var index = mappedType._index ?? i; // If the index was not provided, index by the order in the collection

                _map.Add(index, mappedType._type);

                ++i;
            }
        }

        internal object CreateObject(DbDataReader reader)
        {
            int value = reader.GetInt32(_typeDiscriminatorIndex); // Read the value from the reader

            Type type = _map[value]; // Retrieve the type

            return type.CreateInstance(); // Create an instance from the type
        }
    }
}
