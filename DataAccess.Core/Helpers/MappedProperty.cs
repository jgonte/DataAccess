using System;
using System.Linq.Expressions;
using Utilities;

namespace DataAccess
{
    public class MappedProperty
    {
        internal string _name;

        internal int? _index;

        internal bool _ignore;

        public MappedProperty Name(string name)
        {
            _name = name;

            return this;
        }

        public MappedProperty Map<T>(Expression<Func<T, object>> expression)
        {
            _name = expression.GetPropertyName();      

            return this;
        }

        public MappedProperty Index(int index)
        {
            _index = index;

            return this;
        }

        public MappedProperty Ignore(bool ignore = true)
        {
            _ignore = ignore;

            return this;
        }
    }
}