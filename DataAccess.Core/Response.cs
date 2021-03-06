﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DataAccess
{
    /// <summary>
    /// Encapsulates a response from the database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Response<T>
    {
        /// <summary>
        /// The return code from the database
        /// </summary>
        public int ReturnCode { get; set; }

        /// <summary>
        /// The affected rows in case of a non query action
        /// </summary>
        public int AffectedRows { get; set; }

        /// <summary>
        /// The data returned from the database
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// The parameters retrieved from the command
        /// </summary>
        public List<Parameter> Parameters { get; set; }

        public Parameter GetParameter(string name) => Parameters.SingleOrDefault(p => p._name == name);
        
    }
}
