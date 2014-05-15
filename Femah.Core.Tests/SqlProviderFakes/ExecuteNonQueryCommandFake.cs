using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Femah.Core.Tests.SqlProviderFakes
{
    internal class ExecuteNonQueryCommandFake : CommandMockBase
    {
        private readonly SqlConnectionFake _connectionFake;
        private readonly List<string> _parameters = new List<string>();

        public ExecuteNonQueryCommandFake(SqlConnectionFake connectionFake, Action<List<string>, string> executeAction)
        {
            _connectionFake = connectionFake;

            Command.Setup(x => x.AddParameter(It.Is<SqlParameter>(s => s.ParameterName == "@SwitchName")))
                .Callback<SqlParameter>(x => _parameters.Add(x.Value as string));

            Command.Setup(x => x.ExecuteNonQuery())
                .Callback(() =>
                {
                    _parameters.ForEach(x => executeAction(_connectionFake.Features, x));
                    _parameters.Clear();
                });
        }
    }
}