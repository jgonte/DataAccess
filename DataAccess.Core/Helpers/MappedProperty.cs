using System;
using System.Linq.Expressions;
using Utilities;

namespace DataAccess
{
    public class MappedProperty<T>
    {
        internal string _name;

        internal int? _index;

        internal bool _ignore;

        public MappedProperty<T> Name(string name)
        {
            _name = name;

            return this;
        }

        public MappedProperty<T> Map(Expression<Func<T, object>> expression)
        {
            _name = expression.GetPropertyName();      

            return this;
        }

        public MappedProperty<T> Index(int index)
        {
            _index = index;

            return this;
        }

        public MappedProperty<T> Ignore(bool ignore = true)
        {
            _ignore = ignore;

            return this;
        }
    }
}