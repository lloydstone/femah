using System;
using System.Data.Common;
using Moq;
using Femah.Core.Providers;

namespace Femah.Core.Tests
{
    public class CommandFactory
    {
        public static ISqlCommand CreateNoSwitchesCommand()
        {
            var mockDbReader = Mock.Of<DbDataReader>(x => x.Read() == false);
            var selectAllSwitchesCommand = Mock.Of<ISqlCommand>(x => x.ExecuteReader() == mockDbReader
                && x.ExecuteScalar() == (Object)0);

            return selectAllSwitchesCommand;
        }

        public static ISqlCommand CreateNullRowsCommand()
        {
            var nullReaderMock = new Mock<DbDataReader>();
            nullReaderMock.Setup(s => s.HasRows).Returns(null);
            var nullRowsCommand = new Mock<ISqlCommand>();
            nullRowsCommand.Setup(s => s.ExecuteReader()).Returns(nullReaderMock.Object);
            return nullRowsCommand.Object;
        }

        public static NameRecordingCommand CreateNameRecordingCommand()
        {
            return new NameRecordingCommand();
        }
    }
}