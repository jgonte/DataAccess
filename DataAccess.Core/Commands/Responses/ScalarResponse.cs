using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess
{
    public class ScalarResponse<T> : Response
    {
        /// <summary>
        /// The value returned by the scalar command
        /// </summary>
        public T ReturnValue { get; set; }
    }
}
