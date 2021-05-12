using System.Collections.Generic;
using System.Linq;

namespace DataAccess
{
    public class Response
    {
        /// <summary>
        /// The return code from the database
        /// </summary>
        public int ReturnCode { get; set; }

        /// <summary>
        /// The parameters retrieved from the command
        /// </summary>
        public List<Parameter> Parameters { get; set; }

        public Parameter GetParameter(string name) => Parameters.SingleOrDefault(p => p.Name == name);
    }
}