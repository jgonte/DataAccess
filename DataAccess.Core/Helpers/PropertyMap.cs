using System.Collections.Generic;

namespace DataAccess
{
    public class PropertyMap
    {
        private IDictionary<string, int> _map = new Dictionary<string, int>();

        private HashSet<string> _ignored = new HashSet<string>();

        public PropertyMap(MappedProperty[] mappedProperties)
        {
            var i = 0;

            foreach (var mappedProperty in mappedProperties)
            {
                var index = mappedProperty._index ?? i; // If the index was not provided, index by the order in the collection

                if (mappedProperty._ignore)
                {
                    _ignored.Add(mappedProperty._name);
                }
                else
                {
                    _map.Add(mappedProperty._name, index);
                }

                ++i;
            }
        }

        internal int GetIndex(string name) => _map.ContainsKey(name) ? _map[name] : -1;

        internal bool IsIgnored(string propertyName) => _ignored.Contains(propertyName);
    }
}
