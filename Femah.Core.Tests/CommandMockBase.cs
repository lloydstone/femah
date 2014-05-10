using Femah.Core.Providers;
using Moq;
using System.Data.Common;
using System.Data.SqlClient;

namespace Femah.Core.Tests
{
    internal abstract class CommandMockBase : ISqlCommand
    {
        protected readonly Mock<ISqlCommand> Command = new Mock<ISqlCommand>();

        public DbDataReader ExecuteReader()
        {
            return Command.Object.ExecuteReader();
        }

        public void ExecuteNonQuery()
        {
            Command.Object.ExecuteNonQuery();
        }

        public object ExecuteScalar()
        {
            return Command.Object.ExecuteScalar();
        }

        public void AddParameter(SqlParameter param)
        {
            Command.Object.AddParameter(param);
        }
    }
}