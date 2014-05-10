using Moq;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Femah.Core.Tests
{
    internal class ValueRecordingCommand : CommandMockBase, IEnumerable<string>
    {
        private readonly List<string> _nameList = new List<string>();

        public ValueRecordingCommand()
        {
            Command.Setup(x => x.AddParameter(It.Is<SqlParameter>(s => s.ParameterName == "@SwitchName")))
                .Callback<SqlParameter>(x => _nameList.Add(x.Value as string));
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