namespace Femah.Core.Providers
{
    public interface ISqlConnectionFactory
    {
        ISqlConnection CreateConnection(string connectionString);
    }
}