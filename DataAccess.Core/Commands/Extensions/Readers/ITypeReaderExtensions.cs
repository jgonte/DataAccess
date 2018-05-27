using System;
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
                    var mappedProperties = new List<MappedProperty<U>>();

                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        mappedProperties.Add(new MappedProperty<U>
                        {
                            _name = reader.GetName(i),
                            _index = i
                        });
                    }

                    r.PropertyMap = new PropertyMap<U>(mappedProperties.ToArray());
                }

                var ta = obj.GetTypeAccessor();

                foreach (var pa in ta.PropertyAccessors.Values.Where(a => a.IsPrimitive))
                {
                    if (r.PropertyMap.IsIgnored(pa.PropertyName))
                    {
                        continue; // Do nothing
                    }

                    if (pa.CanSet) // Can set the value in the property of the object
                    {
                        var i = r.PropertyMap.GetIndex(pa.PropertyName);

                        if (i == -1) // Property no mapped
                        {
                            continue;
                        }

                        object value = reader.IsDBNull(i) ? null : reader[i];

                        pa.SetValue(obj, value);
                    }
                }
            }
        }
    }
}
