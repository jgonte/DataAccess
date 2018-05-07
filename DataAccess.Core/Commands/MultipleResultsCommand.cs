using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace DataAccess
{
    public class MultipleResultsCommand : Command
    {
        private Queue<ResultSet> _resultSets = new Queue<ResultSet>();

        public int Execute(Context context = null)
        {
            return ExecuteCommand(context);
        }

        public async Task<int> ExecuteAsync(Context context = null)
        {
            return await ExecuteCommandAsync(context);
        }

        protected override int OnExecute(DbCommand command)
        {
            var count = 0;

            using (var reader = command.ExecuteReader())
            {
                foreach (var query in _resultSets)
                {
                    count += query.Read(reader);

                    reader.NextResult();
                }
            }

            return count;
        }

        protected override async Task<int> OnExecuteAsync(DbCommand command)
        {
            int count = 0;

            using (var reader = await command.ExecuteReaderAsync())
            {
                foreach (var query in _resultSets)
                {
                    count += query.Read(reader);

                    reader.NextResult();
                }
            }

            return count;
        }

        #region Fluent methods

        public MultipleResultsCommand ResultSets(params ResultSet[] resultSets)
        {
            foreach (var resultSet in resultSets)
            {
                _resultSets.Enqueue(resultSet);
            }

            return this;
        } 

        #endregion
    }
}
