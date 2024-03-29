﻿using System.Data.Common;
using System;
using System.Threading.Tasks;

namespace DataAccess
{
    /// <summary>
    /// A command that returns a single value when executed
    /// </summary>
    public class ScalarCommand<T> : Command
    {
        public T ReturnValue { get; private set; }

        public ScalarResponse<T> Execute(Context context = null)
        {
            ExecuteCommand(context);

            return new ScalarResponse<T>
            {
                ReturnCode = ReturnCode,
                Parameters = Parameters,
                ReturnValue = ReturnValue           
            };
        }

        public async Task<ScalarResponse<T>> ExecuteAsync(Context context = null)
        {
            await ExecuteCommandAsync(context);

            return new ScalarResponse<T>
            {
                ReturnCode = ReturnCode,
                Parameters = Parameters,
                ReturnValue = ReturnValue
            };
        }

        protected override int OnExecute(DbCommand command)
        {
            object returnValue = command.ExecuteScalar();

            if (returnValue == null)
            {
                return 0;
            }

            ReturnValue = (T)Convert.ChangeType(returnValue, typeof(T));

            return 0; // Assume no rows were modified
        }

        protected override async Task<int> OnExecuteAsync(DbCommand command)
        {
            object returnValue = await command.ExecuteScalarAsync();

            if (returnValue == null)
            {
                return 0;
            }

            ReturnValue = (T)Convert.ChangeType(returnValue, typeof(T));

            return 0; // Assume no rows were modified
        }
    }
}
