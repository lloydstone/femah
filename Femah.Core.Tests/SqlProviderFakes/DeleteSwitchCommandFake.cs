namespace Femah.Core.Tests.SqlProviderFakes
{
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using Moq;

    internal class DeleteSwitchCommandFake : CommandMockBase
    {
        private readonly SqlConnectionFake _connectionFake;
        private readonly List<string> _names = new List<string>();

        public DeleteSwitchCommandFake(SqlConnectionFake connectionFake)
        {
            _connectionFake = connectionFake;

            Command.Setup(x => x.AddParameter(It.Is<SqlParameter>(s => s.ParameterName == "@SwitchName")))
                .Callback<SqlParameter>(x => _names.Add(x.Value as string));

            Command.Setup(x => x.ExecuteNonQuery())
                .Callback(() =>
                {
                    _connectionFake.Features.RemoveAll(x => _names.Contains(x.Name));
                    _names.Clear();
                });
        }
    }
}