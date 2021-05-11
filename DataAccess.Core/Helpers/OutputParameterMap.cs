using Utilities;

namespace DataAccess
{
    public class OutputParameterMap : INamed
    {
        /// <summary>
        /// The name of the parameter to take the value from
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The name of the property of the entity to populate
        /// </summary>
        public string Property { get; set; }
    }
}