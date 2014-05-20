using Moq;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace Femah.Core.Tests.SqlProviderFakes
{
    internal class SelectSwitchCommandFake : CommandMockBase
    {
        private readonly SqlConnectionFake _connectionFake;
        private string _switchToSelect;
        private bool _isFirstRead = true;

        public SelectSwitchCommandFake(SqlConnectionFake sqlConnectionFake)
        {
            _connectionFake = sqlConnectionFake;
            Command.Setup(x => x.ExecuteReader()).Returns(CreateDataReader);
            Command.Setup(x => x.AddParameter(It.IsAny<SqlParameter>()))
                .Callback<SqlParameter>(x => _switchToSelect = x.Value as string);
        }

        private DbDataReader CreateDataReader()
        {
            var dataReader = new Mock<DbDataReader>();
            dataReader.Setup(s => s.HasRows)
                .Returns(() => _connectionFake.Features.Any(x => x.Name == _switchToSelect));
            dataReader.Setup(x => x.Read()).Returns(() =>
            {
                var currentState = _isFirstRead;
                _isFirstRead = false;
                return currentState;
            });

            dataReader.SetupGet(x => x["name"])
                .Returns(() => _connectionFake.Features.First(x => x.Name == _switchToSelect).Name);

            dataReader.SetupGet(x => x["assemblyName"])
                .Returns(() => _connectionFake.Features.First(x => x.Name == _switchToSelect).AssemblyName);

            dataReader.SetupGet(x => x["isEnabled"])
                .Returns(() => _connectionFake.Features.First(x => x.Name == _switchToSelect).IsEnabled);

            return dataReader.Object;
        }
    }
}