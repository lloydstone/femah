using Moq;
using System.Data.Common;
using System.Linq;

namespace Femah.Core.Tests.SqlProviderFakes
{
    internal class SelectAllSwitchesCommandFake : CommandMockBase
    {
        private readonly SqlConnectionFake _connectionFake;
        private int _dataPointer = -1;

        public SelectAllSwitchesCommandFake(SqlConnectionFake sqlConnectionFake)
        {
            _connectionFake = sqlConnectionFake;
            Command.Setup(x => x.ExecuteReader()).Returns(CreateDataReader);
        }

        private DbDataReader CreateDataReader()
        {
            var dataReader = new Mock<DbDataReader>();
            dataReader.Setup(s => s.HasRows).Returns(() => _connectionFake.Features.Any());
            dataReader.Setup(x => x.Read()).Returns(() =>
            {
                _dataPointer++;
                return _dataPointer < _connectionFake.Features.Count;
            });

            dataReader.SetupGet(x => x["name"]).Returns(() => _connectionFake.Features[_dataPointer].Name);
            return dataReader.Object;
        }
    }
}