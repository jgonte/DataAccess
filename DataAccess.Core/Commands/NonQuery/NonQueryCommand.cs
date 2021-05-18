using System;
using System.Data.Common;
using System.Linq;
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

        public NonQueryResponse Execute(Context context = null)
        {
            ExecuteCommand(context);

            EnsureDatabaseWasUpdated();

            return new NonQueryResponse
            {
                ReturnCode = ReturnCode,
                Parameters = Parameters,
                AffectedRows = AffectedRows
            };
        }

        public async Task<NonQueryResponse> ExecuteAsync(Context context = null)
        {
            await ExecuteCommandAsync(context);

            EnsureDatabaseWasUpdated();

            return new NonQueryResponse
            {
                ReturnCode = ReturnCode,
                Parameters = Parameters,
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

        private void EnsureDatabaseWasUpdated()
        {
            if (AffectedRows == 0 && ThrowWhenNoRecordIsUpdated)
            {
                var rowVersionParameter = Parameters.Where(p => p.Name == "rowVersion").SingleOrDefault();

                if (rowVersionParameter != null &&
                    rowVersionParameter.Value == DBNull.Value)
                {
                    throw new DbConcurrencyException();
                }

                throw new DbNoUpdatedException();
            }
        }
    }
}
