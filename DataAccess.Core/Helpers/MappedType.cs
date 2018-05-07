using System;

namespace DataAccess
{
    public class MappedType
    {
        internal Type _type;

        internal int? _index;

        public MappedType Type(Type type)
        {
            _type = type;

            return this;
        }

        public MappedType Index(int index)
        {
            _index = index;

            return this;
        }
    }
}