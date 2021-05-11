using System.Data;
using Utilities;

namespace DataAccess
{
    /// <summary>
    /// Database agnostic parameter information.
    /// It can be added to the array of parameters without requiring knowledge of the database type.
    /// At the moment of execution, the database specific parameter will be created
    /// </summary>
    public class Parameter : INamed
    {
        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of the parameter
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// The direction of the parameter
        /// </summary>
        public ParameterDirection Direction { get; set; } = ParameterDirection.Input;

        /// <summary>
        /// The size of the data type for IN OUT parameters of types that require size or length
        /// </summary>
        public int? Size { get; set; }

        /// <summary>
        /// The SQL data type of the parameter
        /// </summary>
        public int? SqlType { get; set; }

        public bool IsOutput => Direction == ParameterDirection.Output;

        public bool IsInputOutput => Direction == ParameterDirection.InputOutput;

        public override string ToString()
        {
            return $"Name: '{Name}', SqlType: '{SqlType}' Value: '{Value}', Direction: '{Direction}'";
        }
    }
}
