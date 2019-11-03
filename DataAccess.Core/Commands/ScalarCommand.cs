using System.Data.Common;
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

        public Response<T> Execute(Context context = null)
        {
            ExecuteCommand(context);

            return new Response<T>
            {
                Data = ReturnValue,

                Parameters = Parameters
            };
        }

        public async Task<Response<T>> ExecuteAsync(Context context = null)
        {
            await ExecuteCommandAsync(context);

            return new Response<T>
            {
                Data = ReturnValue,

                Parameters = Parameters
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
