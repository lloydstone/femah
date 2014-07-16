using Moq;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Femah.Core.Tests.SqlProviderFakes
{
    internal class InsertSwitchCommandFake : CommandMockBase
    {
        private readonly SqlConnectionFake _connectionFake;
        private readonly List<string> _names = new List<string>();

        public InsertSwitchCommandFake(SqlConnectionFake connectionFake)
        {
            _connectionFake = connectionFake;

            Command.Setup(x => x.AddParameter(It.Is<SqlParameter>(s => s.ParameterName == "@SwitchName")))
                .Callback<SqlParameter>(x => _names.Add(x.Value as string));

            Command.Setup(x => x.ExecuteNonQuery())
                .Callback(() =>
                {
                    _names.ForEach(x => _connectionFake.Features.Add(Switch.CreateSimpleSwitch(x)));
                    _names.Clear();
                });
        }
    }
}