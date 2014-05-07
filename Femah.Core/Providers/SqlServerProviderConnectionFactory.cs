namespace Femah.Core.Providers
{
    public class SqlServerProviderConnectionFactory : ISqlConnectionFactory
    {
        public ISqlConnection CreateConnection(string connectionString)
        {
            return new SqlServerProviderConnection(connectionString);
        }
    }
}