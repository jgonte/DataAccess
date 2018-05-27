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
        internal DatabaseDriver _driver;

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
        /// The timeout in seconds
        /// </summary>
        internal int? _timeout;

        /// <summary>
        /// Action to execute before the command has been executed
        /// </summary>
        internal Action _onBeforeCommandExecuted;

        /// <summary>
        /// Action to execute when the command has been executed
        /// </summary>
        internal Action _onAfterCommandExecuted;

        /// <summary>
        /// Executes a command
        /// </summary>
        /// <param name="context">The data access context</param>
        /// <returns></returns>
        internal int ExecuteCommand(Context context = null)
        {
            _onBeforeCommandExecuted?.Invoke(); // Set any values before gnenerating the parameters

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
            _onBeforeCommandExecuted?.Invoke(); // Set any values before gnenerating the parameters

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

                connection.Open();

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
                        _name = pa.PropertyName.ToCamelCase(),
                        _value = pa.GetValue(_qbeObject)
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
            using (var command = connection.CreateCommand(_type, _text, Parameters, _timeout, _driver))
            {
                if (transaction != null)
                {
                    command.Transaction = transaction;
                }

                DbParameter returnParameter = null;

                var useReturnValue = command.CommandType == CommandType.StoredProcedure;

                if (useReturnValue)
                {
                    // Create a parameter to store the return code
                    returnParameter = command.CreateParameter();

                    returnParameter.Direction = ParameterDirection.ReturnValue;

                    returnParameter.DbType = DbType.Int32;

                    command.Parameters.Add(returnParameter);
                }

                var rc = OnExecute(command);

                CopyOutParametersValue(command);

                if (useReturnValue)
                {
                    ReturnCode = returnParameter.Value != null ? (int)returnParameter.Value : 0;
                }

                _onAfterCommandExecuted?.Invoke();

                return rc;
            }
        }

        private async Task<int> ExecuteCommandAsync(DbTransaction transaction, DbConnection connection)
        {
            using (DbCommand command = connection.CreateCommand(_type, _text, Parameters, _timeout, _driver))
            {
                if (transaction != null)
                {
                    command.Transaction = transaction;
                }

                DbParameter returnParameter = null;

                bool useReturnValue = command.CommandType == CommandType.StoredProcedure;

                if (useReturnValue)
                {
                    // Create a parameter to store the return code
                    returnParameter = command.CreateParameter();

                    returnParameter.Direction = ParameterDirection.ReturnValue;

                    returnParameter.DbType = DbType.Int32;

                    command.Parameters.Add(returnParameter);
                }

                int rc = await OnExecuteAsync(command);

                CopyOutParametersValue(command);

                if (useReturnValue)
                {
                    ReturnCode = returnParameter.Value != null ? (int)returnParameter.Value : 0;
                }

                _onAfterCommandExecuted?.Invoke();

                return rc;
            }
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

            IDictionary<string, Parameter> parameters = Parameters.ToDictionary(p => _driver.ParameterPlaceHolder + p._name);

            foreach (DbParameter parameter in command.Parameters)
            {
                if (parameter.Direction != ParameterDirection.Input
                    && parameter.Direction != ParameterDirection.ReturnValue)
                {
                    string name = parameter.ParameterName;

                    parameters[name]._value = parameter.Value;
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
