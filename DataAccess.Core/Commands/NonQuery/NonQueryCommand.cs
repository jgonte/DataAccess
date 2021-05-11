using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace DataAccess
{
    /// <summary>
    /// A database command to execute non queries
    /// </summary>
    public class NonQueryCommand : Command
    {
        public int AffectedRows { get; private set; }

        public bool ThrowWhenNoRecordIsUpdated { get; set; } = true;

        public Response<EmptyType> Execute(Context context = null)
        {
            ExecuteCommand(context);

            return new Response<EmptyType>
            {
                AffectedRows = AffectedRows,

                Parameters = Parameters
            };
        }

        public async Task<Response<EmptyType>> ExecuteAsync(Context context = null)
        {
            await ExecuteCommandAsync(context);           

            return new Response<EmptyType>
            {
                AffectedRows = AffectedRows,

                Parameters = Parameters
            };
        }

        protected override int OnExecute(DbCommand command)
        {
            AffectedRows = command.ExecuteNonQuery();

            if (AffectedRows == 0 && ThrowWhenNoRecordIsUpdated)
            {
                throw new InvalidOperationException("No record was updated");
            }

            return AffectedRows;
        }

        protected override async Task<int> OnExecuteAsync(DbCommand command)
        {
            AffectedRows = await command.ExecuteNonQueryAsync();

            if (AffectedRows == 0 && ThrowWhenNoRecordIsUpdated)
            {
                throw new InvalidOperationException("No record was updated");
            }

            return AffectedRows;
        }
    }
}
