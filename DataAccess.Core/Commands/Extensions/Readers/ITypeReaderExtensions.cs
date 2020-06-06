using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Utilities;

namespace DataAccess
{
    public static class ITypeReaderExtensions
    {
        internal static void ReadRecord<T, U>(this T r, DbDataReader reader, U obj)
            where T : ITypeReader<U>
        {
            if (r.OnRecordRead != null) // Use the provided action to populate the object
            {
                r.OnRecordRead(reader, obj); 
            }
            else // Use an existing or create a  property map using reflection
            {
                if (r.PropertyMap == null) // Create a map by matching the name of the returned field with the name of the property
                {
                    var mappedProperties = new List<MappedProperty>();

                    for (var i = 0; i < reader.FieldCount; ++i)
                    {
                        mappedProperties.Add(new MappedProperty
                        {
                            _name = reader.GetName(i),
                            _index = i
                        });
                    }

                    r.PropertyMap = new PropertyMap(mappedProperties.ToArray());
                }

                ReadProperties(reader, r.PropertyMap, obj, null);
            }
        }

        private static void ReadProperties(DbDataReader reader, PropertyMap propertyMap, object obj, string previousPropertyName)
        {
            var typeAccessor = obj.GetTypeAccessor();

            var propertyAccessors = typeAccessor
                .PropertyAccessors.Values
                .Where(pa => !pa.PropertyType.IsCollection());

            foreach (var propertyAccessor in propertyAccessors)
            {
                var propertyName = string.IsNullOrWhiteSpace(previousPropertyName) ? 
                    propertyAccessor.PropertyName : 
                    $"{previousPropertyName}.{propertyAccessor.PropertyName}";

                if (propertyMap.IsIgnored(propertyName))
                {
                    continue; // Do nothing
                }

                if (propertyAccessor.IsPrimitive)
                {
                    if (propertyAccessor.CanSet) // Can set the value in the property of the object
                    {
                        var i = propertyMap.GetIndex(propertyName);

                        if (i == -1) // Property no mapped
                        {
                            continue;
                        }

                        object value = reader.IsDBNull(i) ? null : reader[i];

                        propertyAccessor.SetValue(obj, value);
                    }
                }
                else // Nested property
                {
                    var o = propertyAccessor.PropertyType.CreateInstance();

                    ReadProperties(reader, propertyMap, o, propertyName);

                    propertyAccessor.SetValue(obj, o);
                }
            }
        }
    }
}
