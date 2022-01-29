using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using System.Transactions;

namespace DataAccess
{
    /// <summary>
    /// Executes a set of commands under a database transaction
    /// </summary>
    public class Transaction
    {
        public enum Modes
        {
            Local = 0,
            Distributed
        };

        /// <summary>
        /// Whether the transaction is local cc distributed
        /// </summary>
        internal Modes _mode;

        public enum IsolationLevels
        {
            Unspecified = 0,
            Chaos,
            ReadUncommitted,
            ReadCommitted,
            RepeatableRead,
            Serializable,
            Snapshot,
        }

        /// <summary>
        /// The isolation level of the transaction
        /// </summary>
        internal IsolationLevels _isolationLevel;

        public enum Scopes
        {
            Required = 0,
            RequiresNew,
            Suppress
        }

        /// <summary>
        /// The scope used only for distributed transactions (for local ones is ignored)
        /// </summary>
        internal Scopes _scope;

        /// <summary>
        /// In case of a distributed transaction, this connection is applied to the commands that do not have any connection assigned
        /// In case of a local transaction, this connection is used for the transaction. It must not be null. If any command has an
        /// assigned connections an exception is thrown since local transactions can only use one connection
        /// </summary>
        internal Connection _connection;

        /// <summary>
        /// The database commands to be executed under the transaction
        /// </summary>
        internal Queue<Command> _commands = new Queue<Command>();

        public void Execute()
        {
            if (_mode == Modes.Distributed)
            {
                ExecuteDistributedTransaction();
            }
            else
            {
                ExecuteLocalTransaction();
            }
        }

        public async Task ExecuteAsync()
        {
            if (_mode == Modes.Distributed)
            {
                await ExecuteDistributedTransactionAsync();
            }
            else
            {
                await ExecuteLocalTransactionAsync();
            }
        }

        #region Helpers

        /// <summary>
        /// Executes a distributed transaction (different database servers)
        /// </summary>
        private void ExecuteDistributedTransaction()
        {
            using (TransactionScope transactionScope = new TransactionScope(GetScope(), new TransactionOptions { IsolationLevel = GetDistributedIsolationLevel() }))
            {
                foreach (Command command in _commands)
                {
                    command.ExecuteCommand(null);
                }

                transactionScope.Complete();
            }
        }

        /// <summary>
        /// Executes a distributed transaction (different database servers)
        /// </summary>
        private async Task ExecuteDistributedTransactionAsync()
        {
            using (TransactionScope transactionScope = new TransactionScope(GetScope(), new TransactionOptions { IsolationLevel = GetDistributedIsolationLevel() }))
            {
                foreach (Command command in _commands)
                {
                    await command.ExecuteCommandAsync(null);
                }

                transactionScope.Complete();
            }
        }

        private TransactionScopeOption GetScope()
        {
            switch (_scope)
            {
                case Scopes.Required: return TransactionScopeOption.Required;
                case Scopes.RequiresNew: return TransactionScopeOption.RequiresNew;
                case Scopes.Suppress: return TransactionScopeOption.Suppress;
                default: throw new ArgumentException($"Unknown transaction scope: {_scope}"); // In case a scope is added
            }
        }

        /// <summary>
        /// Converts to a distributed isolation level
        /// </summary>
        /// <returns></returns>
        private IsolationLevel GetDistributedIsolationLevel()
        {
            switch (_isolationLevel)
            {
                case IsolationLevels.Unspecified: return IsolationLevel.Unspecified;
                case IsolationLevels.Chaos: return IsolationLevel.Chaos;
                case IsolationLevels.ReadUncommitted: return IsolationLevel.ReadUncommitted;
                case IsolationLevels.ReadCommitted: return IsolationLevel.ReadCommitted;
                case IsolationLevels.RepeatableRead: return IsolationLevel.RepeatableRead;
                case IsolationLevels.Serializable: return IsolationLevel.Serializable;
                case IsolationLevels.Snapshot: return IsolationLevel.Snapshot;
                default: throw new ArgumentException($"Unknown isolation level: {_isolationLevel}"); // In case a new isolation level is added
            }
        }

        /// <summary>
        /// Executes a local transaction (for the same database)
        /// </summary>
        private void ExecuteLocalTransaction()
        {
            var providerFactory = DbProviderFactories.GetFactory(_connection.ProviderName);

            using (DbConnection connection = providerFactory.CreateConnection())
            {
                connection.ConnectionString = _connection.ConnectionString;

                connection.Open();

                var transaction = connection.BeginTransaction(GetLocalIsolationLevel());

                Command executingCommand; // To quickly know what command is failing

                try
                {
                    foreach (Command command in _commands)
                    {
                        executingCommand = command;

                        // Make sure the command has the database driver set
                        if (command.DatabaseDriver == null)
                        {
                            command.DatabaseDriver = DatabaseDriverManager.Drivers[_connection.ProviderName];
                        }

                        command.ExecuteCommand(new Context
                        {
                            Connection = connection,
                            Transaction = transaction
                        });
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();

                    //executingCommand // Check the executing command here

                    throw;
                }
            }
        }

        /// <summary>
        /// Executes a local transaction (for the same database)
        /// </summary>
        private async Task ExecuteLocalTransactionAsync()
        {
            var providerFactory = DbProviderFactories.GetFactory(_connection.ProviderName);

            using (DbConnection connection = providerFactory.CreateConnection())
            {
                connection.ConnectionString = _connection.ConnectionString;

                await connection.OpenAsync();

                var transaction = connection.BeginTransaction(GetLocalIsolationLevel());

                Command executingCommand; // To quickly know what command is failing

                try
                {
                    var tasks = new Queue<Task>();

                    foreach (var command in commands)
                    {
                        executingCommand = command;

                        // Make sure the command has the database driver set
                        if (command.DatabaseDriver == null)
                        {
                            command.DatabaseDriver = DatabaseDriverManager.Drivers[_connection.ProviderName];
                        }

                        tasks.Enqueue(
                            command.ExecuteCommandAsync(new Context
                            {
                                Connection = connection,
                                Transaction = transaction
                            })
                        );

                        await Task.WhenAll(tasks);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();

                    //executingCommand // Check the executing command here

                    throw;
                }
            }
        }

        private System.Data.IsolationLevel GetLocalIsolationLevel()
        {
            switch (_isolationLevel)
            {
                case IsolationLevels.Unspecified: return System.Data.IsolationLevel.Unspecified;
                case IsolationLevels.Chaos: return System.Data.IsolationLevel.Chaos;
                case IsolationLevels.ReadUncommitted: return System.Data.IsolationLevel.ReadUncommitted;
                case IsolationLevels.ReadCommitted: return System.Data.IsolationLevel.ReadCommitted;
                case IsolationLevels.RepeatableRead: return System.Data.IsolationLevel.RepeatableRead;
                case IsolationLevels.Serializable: return System.Data.IsolationLevel.Serializable;
                case IsolationLevels.Snapshot: return System.Data.IsolationLevel.Snapshot;
                default: throw new ArgumentException($"Unknown isolation level: {_isolationLevel}"); // In case a new isolation level is added
            }
        }

        #endregion

        #region Fluent methods

        #region Isolation levels

        public Transaction Unspecified()
        {
            _isolationLevel = IsolationLevels.Unspecified;

            return this;
        }

        public Transaction Chaos()
        {
            _isolationLevel = IsolationLevels.Chaos;

            return this;
        }

        public Transaction ReadUncommitted()
        {
            _isolationLevel = IsolationLevels.ReadUncommitted;

            return this;
        }

        public Transaction ReadCommitted()
        {
            _isolationLevel = IsolationLevels.ReadCommitted;

            return this;
        }

        public Transaction RepeatableRead()
        {
            _isolationLevel = IsolationLevels.RepeatableRead;

            return this;
        }

        public Transaction Serializable()
        {
            _isolationLevel = IsolationLevels.Serializable;

            return this;
        }

        public Transaction Snapshot()
        {
            _isolationLevel = IsolationLevels.Snapshot;

            return this;
        }
        
        #endregion

        public Transaction Connection(string connectionName)
        {
            _connection = ConnectionManager.GetConnection(connectionName);

            return this;
        }

        public Transaction Commands(params Command[] commands)
        {
            foreach (var command in commands)
            {
                _commands.Enqueue(command);
            }
            
            return this;
        }

        #endregion

        #region Factory methods

        public static Transaction Local()
        {
            return new Transaction
            {
                _mode = Modes.Local
            };
        }

        public static Transaction Distributed(Scopes scope = Scopes.Required)
        {
            return new Transaction
            {
                _mode = Modes.Distributed,

                _scope = scope
            };
        } 

        #endregion
    }
}
