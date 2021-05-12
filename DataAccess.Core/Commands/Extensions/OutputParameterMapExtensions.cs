using System;
using System.Linq.Expressions;
using Utilities;

namespace DataAccess
{
    public static class OutputParameterMapExtensions
    {
        public static OutputParameterMap Property(this OutputParameterMap parameterMap, string propertyName)
        {
            parameterMap.Property = propertyName;

            return parameterMap;
        }

        public static OutputParameterMap Property<T>(this OutputParameterMap parameterMap, Expression<Func<T, object>> expression)
        {
            parameterMap.Property = expression.GetPropertyName();

            return parameterMap;
        }
    }
}
