using System.Data;

namespace DataAccess
{
    /// <summary>
    /// Database agnostic parameter information.
    /// It can be added to the array of parameters without requiring knowledge of the database type.
    /// At the moment of execution, the database specific parameter will be created
    /// </summary>
    public class Parameter
    {
        public Parameter()
        {
            _direction = ParameterDirection.Input;
        }

        /// <summary>
        /// The name of the parameter
        /// </summary>
        internal string _name;

        /// <summary>
        /// The value of the parameter
        /// </summary>
        internal object _value;

        /// <summary>
        /// The size of the data type for IN OUT parameters of type string
        /// </summary>
        internal int? _size;

        /// <summary>
        /// The sql type of the parameter
        /// </summary>
        internal int? _type;

        /// <summary>
        /// The direction of the parameter
        /// </summary>
        internal ParameterDirection _direction;

        public object Value => _value;

        public override string ToString()
        {
            return string.Format("Name: '{0}', Value: '{1}', Direction: '{2}'", _name, _value, _direction);
        }

        #region Fluent methods

        public Parameter Name(string name)
        {
            _name = name;

            return this;
        }

        public Parameter IsInput()
        {
            _direction = ParameterDirection.Input;

            return this;
        }

        public Parameter Output()
        {
            _direction = ParameterDirection.Output;

            return this;
        }

        public Parameter InputOutput()
        {
            _direction = ParameterDirection.InputOutput;

            return this;
        }

        public Parameter IsReturnValue()
        {
            _direction = ParameterDirection.ReturnValue;

            return this;
        }

        public Parameter Size(int size)
        {
            _size = size;

            return this;
        }

        #endregion
    }

    /// <summary>
    /// Extension class to add an extension method so we can have the fluent method as an extension as well as a property with the same name
    /// </summary>
    public static class ParameterExtensions
    {
        public static Parameter Value(this Parameter parameter, object value)
        {
            parameter._value = value;

            return parameter;
        }
    }
}
