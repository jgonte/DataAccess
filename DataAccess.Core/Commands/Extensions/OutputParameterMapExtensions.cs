namespace DataAccess
{
    public static class OutputParameterMapExtensions
    {
        public static OutputParameterMap Property(this OutputParameterMap parameterMap, string propertyName)
        {
            parameterMap.Property = propertyName;

            return parameterMap;
        }
    }
}
