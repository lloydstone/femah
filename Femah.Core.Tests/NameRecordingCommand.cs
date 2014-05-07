using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using Moq;
using Femah.Core.Providers;

namespace Femah.Core.Tests
{
    public class NameRecordingCommand : IEnumerable<string>
    {
        private readonly List<string> _nameList = new List<string>();
        private readonly Mock<ISqlCommand> _command = new Mock<ISqlCommand>();

        public NameRecordingCommand()
        {
            _command.Setup(x => x.AddParameter(It.Is<SqlParameter>(s => s.ParameterName == "@SwitchName")))
                .Callback<SqlParameter>(x => _nameList.Add(x.Value as string));
        }

        public ISqlCommand ToISqlCommand()
        {
            return _command.Object;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _nameList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}