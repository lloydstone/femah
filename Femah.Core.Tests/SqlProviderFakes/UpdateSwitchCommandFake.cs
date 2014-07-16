namespace Femah.Core.Tests.SqlProviderFakes
{
    internal class UpdateSwitchCommandFake : CommandMockBase
    {
        private readonly SqlConnectionFake _connectionFake;

        public UpdateSwitchCommandFake(SqlConnectionFake connectionFake)
        {
            _connectionFake = connectionFake;
        }
    }
}