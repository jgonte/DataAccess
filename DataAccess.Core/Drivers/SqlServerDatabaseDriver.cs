using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace DataAccess
{
    class SqlServerDatabaseDriver : DatabaseDriver
    {
        public override string ParameterPlaceHolder
        {
            get { return "@"; }
        }

        public override void SetParameterType(DbParameter parameter, int type)
        {
            SqlParameter p = (SqlParameter)parameter;

            p.SqlDbType = (SqlDbType)type;
        }
    }
}
