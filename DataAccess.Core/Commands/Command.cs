using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace DataAccess
{
    /// <summary>
    /// Database agnostic command
    /// </summary>
    public abstract class Command
    {
        public int ReturnCode { get; set; }

        /// <summary>
        /// The driver to perform database specific operations
        /// </summary>
        public DatabaseDriver DatabaseDriver { get; internal set; }

        internal Connection _connection;

        internal CommandType _type;

        /// <summary>
        /// The SQL statement to execute
        /// </summary>
        internal string _text;

        /// <summary>
        /// Whether to generate the parameters automatically from the instance of the object
        /// </summary>
        internal bool _autoGenerateParameters;

        /// <summary>
        /// The properties to exclude during the generation of the parameters
        /// </summary>
        internal string[] _excludedPropertiesInParametersGeneration;

        /// <summary>
        /// The Query By Example object from which the parameters are generated
        /// </summary>
        internal object _qbeObject;

        /// <summary>
        /// The parameters to use
        /// </summary>
        // Need to be public accessable to retrieve the values of output parameters
        public List<Parameter> Parameters { get; internal set; } = new List<Parameter>();

        /// <summary>
        /// Maps the output parameters to the properties of the entity
        /// </summary>
        public List<OutputParameterMap> OutputParameterMaps { get; internal set; } = new List<OutputParameterMap>();

        /// <summary>
        /// The timeout in seconds
        /// </summary>
        internal int? _timeout;

        /// <summary>
        /// Action to execute before the command has been executed
        /// </summary>
        internal Action<Command> _onBeforeCommandExecuted;

        /// <summary>
        /// Action to execute when the command has been executed
        /// </summary>
        internal Action<Command> _onAfterCommandExecuted;

        /// <summary>
        /// The instance of the entity to populate the parameters from or pass values to from the output parameters using the map
        /// </summary>
        public object Entity { get; set; }

        /// <summary>
        /// Executes a command
        /// </summary>
        /// <param name="context">The data access context</param>
        /// <returns></returns>
        internal int ExecuteCommand(Context context = null)
        {
            _onBeforeCommandExecuted?.Invoke(this); // Set any values before generating the parameters

            if (_autoGenerateParameters)
            {
                GenerateParameters();
            }

            if (context != null && context.Connection != null) // It is coming from a transaction
            {
                return ExecuteCommand(context.Transaction, context.Connection);
            }

            if (null == _connection)
            {
                throw new ArgumentNullException("Connection");
            }

            // If we are here that means that the command is not executing under a transaction
            var providerFactory = DbProviderFactories.GetFactory(_connection.ProviderName);

            using (var connection = providerFactory.CreateConnection())
            {
                connection.ConnectionString = _connection.ConnectionString;

                connection.Open();

                return ExecuteCommand(null, connection);
            }
        }

        internal async Task<int> ExecuteCommandAsync(Context context = null)
        {
            _onBeforeCommandExecuted?.Invoke(this); // Set any values before generating the parameters

            if (_autoGenerateParameters)
            {
                GenerateParameters();
            }

            if (context != null && context.Connection != null) // It is coming from a transaction
            {
                return await ExecuteCommandAsync(context.Transaction, context.Connection);
            }

            if (null == _connection)
            {
                throw new ArgumentNullException("Connection");
            }

            // If we are here that means that the command is not executing under a transaction
            var providerFactory = DbProviderFactories.GetFactory(_connection.ProviderName);

            using (var connection = providerFactory.CreateConnection())
            {
                connection.ConnectionString = _connection.ConnectionString;

                await connection.OpenAsync();

                return await ExecuteCommandAsync(null, connection);
            }
        }

        /// <summary>
        /// Executes a database command
        /// </summary>
        /// <param name="command">The command to be executed</param>
        /// <returns></returns>
        protected abstract int OnExecute(DbCommand command);

        protected abstract Task<int> OnExecuteAsync(DbCommand command);

        #region Helpers

        /// <summary>
        /// Generates the parameters using reflection from a Query By Example object
        /// </summary>
        private void GenerateParameters()
        {
            var ta = _qbeObject.GetTypeAccessor();

            if (_excludedPropertiesInParametersGeneration == null)
            {
                _excludedPropertiesInParametersGeneration = new string[] { };
            }

            foreach (var pa in ta.PropertyAccessors.Values
                .Where(a => a.IsPrimitive &&
                !_excludedPropertiesInParametersGeneration.Contains(a.PropertyName)))
            {
                if (pa.CanGet) // Can get the value in the property of the object
                {
                    Parameters.Add(new Parameter
                    {
                        Name = pa.PropertyName.ToCamelCase(),
                        Value = pa.GetValue(_qbeObject)
                    });
                }
            }
        }

        internal DataTable CreateTable<I>(string typeName, ICollection<I> collection, string columnName)
        {
            DataTable table = new DataTable(typeName);

            Type itemType = typeof(I);

            if (itemType.IsPrimitive())
            {
                if (string.IsNullOrWhiteSpace(columnName))
                {
                    throw new ArgumentNullException("columnName");
                }

                table.Columns.Add(columnName);

                // Add the rows
                foreach (var item in collection)
                {
                    DataRow row = table.NewRow();

                    row[columnName] = item;

                    table.Rows.Add(row);
                }
            }
            else // Use reflection to create the table from the properties of the type
            {
                var accessor = itemType.GetTypeAccessor();

                // Add the columns
                foreach (var property in accessor.PropertyAccessors.Keys)
                {
                    table.Columns.Add(property);
                }

                // Add the rows
                foreach (var item in collection)
                {
                    DataRow row = table.NewRow();

                    foreach (var property in accessor.PropertyAccessors.Keys)
                    {
                        row[property] = accessor.GetValue(item, property);
                    }

                    table.Rows.Add(row);
                }
            }

            return table;
        }

        private int ExecuteCommand(DbTransaction transaction, DbConnection connection)
        {
            using (var command = connection.CreateCommand(_type, _text, Parameters, _timeout, DatabaseDriver))
            {
                var useReturnValue = command.CommandType == CommandType.StoredProcedure;

                var returnParameter = BeforeExecuteCommand(transaction, command, useReturnValue);

                var rc = OnExecute(command);

                AfterExecuteCommand(command, useReturnValue, returnParameter);

                return rc;
            }
        }

        private async Task<int> ExecuteCommandAsync(DbTransaction transaction, DbConnection connection)
        {
            using (DbCommand command = connection.CreateCommand(_type, _text, Parameters, _timeout, DatabaseDriver))
            {
                var useReturnValue = command.CommandType == CommandType.StoredProcedure;

                var returnParameter = BeforeExecuteCommand(transaction, command, useReturnValue);

                int rc = await OnExecuteAsync(command);

                AfterExecuteCommand(command, useReturnValue, returnParameter);

                return rc;
            }
        }

        private static DbParameter BeforeExecuteCommand(DbTransaction transaction, DbCommand command, bool useReturnValue)
        {
            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            DbParameter returnParameter = null;

            if (useReturnValue)
            {
                // Create a parameter to store the return code
                returnParameter = command.CreateParameter();

                returnParameter.Direction = ParameterDirection.ReturnValue;

                returnParameter.DbType = DbType.Int32;

                command.Parameters.Add(returnParameter);
            }

            return returnParameter;
        }

        private void AfterExecuteCommand(DbCommand command, bool useReturnValue, DbParameter returnParameter)
        {
            CopyOutParametersValue(command);

            if (useReturnValue)
            {
                ReturnCode = returnParameter.Value != null ? (int)returnParameter.Value : 0;
            }

            if (OutputParameterMaps.Any())
            {
                if (Entity == null)
                {
                    throw new InvalidOperationException("Entity cannot be null if output parameter maps are configured");
                }

                var accessor = Entity.GetTypeAccessor();

                foreach (var outputParameterMap in OutputParameterMaps)
                {
                    var parameter = Parameters.Where(p => p.Name == outputParameterMap.Name).SingleOrDefault();

                    if (parameter == null)
                    {
                        throw new InvalidOperationException($"Output parameter of name: {outputParameterMap.Name} was not found");
                    }

                    if (!parameter.IsOutput && !parameter.IsInputOutput)
                    {
                        throw new InvalidOperationException($"Output parameter of name: {outputParameterMap.Name} is neither input nor input-output");
                    }

                    accessor.SetValue(Entity, outputParameterMap.Property, parameter.Value);
                }
            }

            _onAfterCommandExecuted?.Invoke(this);
        }


        /// <summary>
        /// Copies the values of the parameters that are not input only to the database independent parameters of this command
        /// </summary>
        /// <param name="command"></param>
        private void CopyOutParametersValue(DbCommand command)
        {
            if (Parameters == null)
            {
                return;
            }

            IDictionary<string, Parameter> parameters = Parameters.ToDictionary(p => DatabaseDriver.ParameterPlaceHolder + p.Name);

            foreach (DbParameter parameter in command.Parameters)
            {
                if (parameter.Direction != ParameterDirection.Input
                    && parameter.Direction != ParameterDirection.ReturnValue)
                {
                    var name = parameter.ParameterName;

                    parameters[name].Value = parameter.Value;
                }
            }
        }

        #endregion

        #region Command factory methods

        public static NonQueryCommand NonQuery() => new NonQueryCommand();

        public static ScalarCommand<T> Scalar<T>() => new ScalarCommand<T>();

        public static MultipleResultsCommand MultipleResults() => new MultipleResultsCommand();

        #endregion
    }
}
