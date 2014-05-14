namespace Femah.Core.Tests.SqlProviderFakes
{
    internal class ExecuteReaderCommandFake : CommandMockBase
    {
        private readonly SqlConnectionFake _connectionFake;

        public ExecuteReaderCommandFake(SqlConnectionFake sqlConnectionFake)
        {
            _connectionFake = sqlConnectionFake;
        }
    }
}