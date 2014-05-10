using System.Collections.Generic;
using System.Collections;
using System.Data.Common;
using Moq;
using System.Linq;

namespace Femah.Core.Tests
{
    internal class ExistingSwitchesCommand : CommandMockBase, IEnumerable<string>
    {
        private readonly IList<string> _switchNames;
        private int _dataPointer = -1;

        public ExistingSwitchesCommand(IList<string> switchNames)
        {
            _switchNames = switchNames;
            Command.Setup(x => x.ExecuteReader()).Returns(CreateDataReader());
        }

        private DbDataReader CreateDataReader()
        {
            var dataReader = new Mock<DbDataReader>();
            dataReader.Setup(x => x.Read()).Returns(() =>
            {
                _dataPointer++;
                return _dataPointer < _switchNames.Count();
            });

            dataReader.SetupGet(x => x["name"]).Returns(() => _switchNames[_dataPointer]);
            return dataReader.Object;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _switchNames.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}