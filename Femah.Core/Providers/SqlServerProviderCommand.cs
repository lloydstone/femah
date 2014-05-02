using System.Data.Common;
using System.Data.SqlClient;

namespace Femah.Core.Providers
{
    public class SqlServerProviderCommand : ISqlCommand
    {
        private readonly SqlCommand _sqlCommand;

        public SqlServerProviderCommand(SqlConnection sqlConnection, string command)
        {
            _sqlCommand = new SqlCommand(command, sqlConnection);
        }

        public DbDataReader ExecuteReader()
        {
            return _sqlCommand.ExecuteReader();
        }

        public void ExecuteNonQuery()
        {
            _sqlCommand.ExecuteNonQuery();
        }

        public object ExecuteScalar()
        {
            return _sqlCommand.ExecuteScalar();
        }

        public void AddParameter(SqlParameter param)
        {
            _sqlCommand.Parameters.Add(param);
        }
    }
}