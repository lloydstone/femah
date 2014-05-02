using System.Data.SqlClient;

namespace Femah.Core.Providers
{
    public class SqlServerProviderConnection : ISqlConnection
    {
        private SqlConnection _connection;

        public SqlServerProviderConnection(string connectionString)
        {
            _connection = new SqlConnection(connectionString);   
        }

        public void Open()
        {
            _connection.Open();
        }

        public ISqlCommand CreateCommand(string command)
        {
            return new SqlServerProviderCommand(_connection, command);
        }

        public void Dispose()
        {
            if (_connection == null) return;
            
            _connection.Dispose();
            _connection = null;
        }
    }
}