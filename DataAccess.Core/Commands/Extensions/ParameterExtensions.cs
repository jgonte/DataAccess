using System.Data;

namespace DataAccess
{
    /// <summary>
    /// Extension class to add an extension method so we can have the fluent method as an extension as well as a property with the same name
    /// </summary>
    public static class ParameterExtensions
    {
        public static Parameter Value(this Parameter parameter, object value)
        {
            parameter.Value = value;

            return parameter;
        }

        public static Parameter Set(this Parameter parameter, string name, object value)
        {
            parameter.Name = name;

            parameter.Value = value;

            return parameter;
        }

        public static Parameter Size(this Parameter parameter, int size)
        {
            parameter.Size = size;

            return parameter;
        }

        public static Parameter SqlType(this Parameter parameter, int sqlType)
        {
            parameter.SqlType = sqlType;

            return parameter;
        }

        public static Parameter IsOutput(this Parameter parameter)
        {
            parameter.Direction = ParameterDirection.Output;

            return parameter;
        }

        public static Parameter IsInputOutput(this Parameter parameter)
        {
            parameter.Direction = ParameterDirection.InputOutput;

            return parameter;
        }

        /// <summary>
        /// To configure an output parameter that returns a counter
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static Parameter Count(this Parameter parameter)
        {
            return parameter.SqlType((int)SqlDbType.Int).IsOutput();
        }

        //public Parameter IsInput()
        //{
        //    _direction = ParameterDirection.Input;

        //    return this;
        //}


        

        //public Parameter IsReturnValue()
        //{
        //    _direction = ParameterDirection.ReturnValue;

        //    return this;
        //}

    }
}
