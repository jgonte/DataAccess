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

        public Response<EmptyType> Execute(Context context = null)
        {
            ExecuteCommand(context);

            return new Response<EmptyType>
            {
                AffectedRows = AffectedRows
            };
        }

        public async Task<Response<EmptyType>> ExecuteAsync(Context context = null)
        {
            await ExecuteCommandAsync(context);

            return new Response<EmptyType>
            {
                AffectedRows = AffectedRows
            };
        }

        protected override int OnExecute(DbCommand command)
        {
            AffectedRows = command.ExecuteNonQuery();

            return AffectedRows;
        }

        protected override async Task<int> OnExecuteAsync(DbCommand command)
        {
            AffectedRows = await command.ExecuteNonQueryAsync();

            return AffectedRows;
        }
    }
}
