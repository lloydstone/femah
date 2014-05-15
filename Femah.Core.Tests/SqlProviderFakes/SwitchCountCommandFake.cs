using Moq;
using System.Data.SqlClient;
using System.Linq;

namespace Femah.Core.Tests.SqlProviderFakes
{
    internal class SwitchCountCommandFake : CommandMockBase
    {
        private readonly SqlConnectionFake _connectionFake;
        private string _switchToSelect;

        public SwitchCountCommandFake(SqlConnectionFake sqlConnectionFake)
        {
            _connectionFake = sqlConnectionFake;
            Command.Setup(x => x.AddParameter(It.IsAny<SqlParameter>()))
                .Callback<SqlParameter>(x => _switchToSelect = x.Value as string);
            Command.Setup(x => x.ExecuteScalar())
                .Returns(() => _connectionFake.Features.Count(x => x == _switchToSelect) as object);
        }
    }
}