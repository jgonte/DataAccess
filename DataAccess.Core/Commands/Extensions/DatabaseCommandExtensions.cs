﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Utilities;

namespace DataAccess
{
    public static class DatabaseCommandExtensions
    {
        #region Fluent methods

        public static T Connection<T>(this T command, string connectionName, DatabaseDriver driver = null)
            where T : Command
        {
            command._connection = ConnectionManager.GetConnection(connectionName);

            command._driver = driver != null ? driver : DatabaseDriverManager.Drivers[command._connection.ProviderName];

            return command;
        }

        public static T ConnectionString<T>(this T command, string connectionString, string providerName, DatabaseDriver driver = null)
            where T : Command
        {
            command._connection = new Connection
            {
                ConnectionString = connectionString,
                ProviderName = providerName
            };

            command._driver = driver != null ? driver : DatabaseDriverManager.Drivers[command._connection.ProviderName];

            return command;
        }

        public static T Text<T>(this T command, string text)
            where T : Command
        {
            command._text = text;

            command._type = CommandType.Text;

            return command;
        }

        public static T StoredProcedure<T>(this T command, string text)
            where T : Command
        {
            command._text = text;

            command._type = CommandType.StoredProcedure;

            return command;
        }

        public static T Timeout<T>(this T command, int timeout)
            where T : Command
        {
            command._timeout = timeout;

            return command;
        }

        public static T Parameter<T>(this T command, string name, object value, int? size = null)
            where T : Command
        {
            command.Parameters.Add(new Parameter
            {
                _name = name,
                _value = value,
                _size = size
            });

            return command;
        }

        public static T Parameters<T>(this T command, params Parameter[] databaseParameters)
            where T : Command
        {
            command.Parameters.AddRange(databaseParameters);

            return command;
        }

        public static T Parameters<T>(this T command, params Action<Parameter>[] configures)
            where T : Command
        {
            return Parameters(command, configures
                .Select(configure =>
                {
                    var parameter = new Parameter();

                    configure(parameter);

                    return parameter;
                })
                .ToArray());
        }

        /// <summary>
        /// Directs this command to automatically generate the parameters from the instance
        /// using reflection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="qbeObject">The object to generate the parameters from</param>
        /// <param name="excludedProperties">The properties that will be excluded from the parameter generation</param>
        /// <returns></returns>
        public static T AutoGenerateParameters<T>(this T command, object qbeObject, string[] excludedProperties = null)
            where T : Command
        {
            command._autoGenerateParameters = true;

            command._qbeObject = qbeObject ?? new ArgumentNullException(nameof(qbeObject));

            command._excludedPropertiesInParametersGeneration = excludedProperties;

            return command;
        }

        public static T AutoGenerateParameters<T, U>(this T command, U qbeObject, Expression<Func<U, object>>[] excludedProperties = null)
            where T : Command
        {
            var excludedProps = new List<string>();

            if (excludedProperties != null)
            {
                foreach (var expression in excludedProperties)
                {
                    excludedProps.Add(expression.GetPropertyName());
                }
            }

            return AutoGenerateParameters(command, qbeObject, excludedProps.ToArray());
        }

        /// <summary>
        /// Adds a table array parameter
        /// </summary>
        /// <typeparam name="I"></typeparam>
        /// <param name="typeName"></param>
        /// <param name="name"></param>
        /// <param name="collection"></param>
        /// <param name="columnName">The name of the column to supply if the parameter is a primitive</param>
        /// <returns></returns>
        public static T Parameter<T, I>(this T command, string typeName, string name, ICollection<I> collection, string columnName = null)
            where T : Command
        {
            // Create the data table from the array
            DataTable table = command.CreateTable(typeName, collection, columnName);

            command.Parameters.Add(new Parameter
            {
                _name = name,
                _type = (int)SqlDbType.Structured,
                _value = table
            });

            return command;
        }

        public static T OnBeforeCommandExecuted<T>(this T command, Action<Command> onBeforeCommandExecuted)
            where T : Command
        {
            command._onBeforeCommandExecuted = onBeforeCommandExecuted;

            return command;
        }

        public static T OnAfterCommandExecuted<T>(this T command, Action<Command> onAfterCommandExecuted)
            where T : Command
        {
            command._onAfterCommandExecuted = onAfterCommandExecuted;

            return command;
        }

        #endregion
    }
}
