using Femah.Core.Providers;
using Moq;
using NUnit.Framework;
using Shouldly;
using System.Linq;

namespace Femah.Core.Tests
{
    public class SqlServerProviderTests
    {
        public class TheConfigureMethod
        {
            [TestCase("teststring")]
            [TestCase("Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;")]
            public void SetsTheConnectionString(string connectionString)
            {
                var sut = new SqlServerProvider(null);
                sut.Configure(connectionString);

                sut.ConnectionString.ShouldBe(connectionString);
            }
        }

        public class TheInitializeMethod
        {
            private SqlServerProvider _sut;
            private Mock<ISqlConnectionFactory> _connectionFactory;
            private Mock<ISqlConnection> _sqlConnection;
            private const string TableName = "femahSwitches";

            [SetUp]
            public void Init()
            {
                _connectionFactory = new Mock<ISqlConnectionFactory>();
                _sqlConnection = new Mock<ISqlConnection>();
                _sut = new SqlServerProvider(_connectionFactory.Object);
            }

            [Test]
            public void WhenSuppliedWithFeatureNames_AndIfDataStoreIsInitiallyEmpty_SetsUpAllNewFeatureSwitchesInDataStore()
            {
                var featureNames = new[] {"Feature1", "Feature2", "Feature3"};
                const string connectionString = "Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;";
                _connectionFactory.Setup(x => x.CreateConnection(connectionString)).Returns(_sqlConnection.Object);

                var nullRowsCommand = CommandFactory.CreateNullRowsCommand();
                AddCommandDefinition(SelectSwitchSql, nullRowsCommand);

                var noSwitchesCommand = CommandFactory.CreateNoSwitchesCommand();
                AddCommandDefinition(SwitchCountSql, noSwitchesCommand);
                AddCommandDefinition(SelectAllSwitchesSql, noSwitchesCommand);

                var nameRecorder = CommandFactory.CreateNameRecordingCommand();
                AddCommandDefinition(InsertSwitchSql, nameRecorder.ToISqlCommand());
                
                _sut.Configure(connectionString);
                _sut.Initialise(featureNames);

                featureNames.ShouldContain(x => nameRecorder.Contains(x));
            }

            // TODO: Calling initialize without setting connection string throws
            // TODO: Sets up passed in switches
            // TODO: removes switched that do not exist in list but do in db
            // TODO: if switch does not exist already, simple switch is created
            // TODO: if switch does exist, it is updated

            private void AddCommandDefinition(string commandSql, ISqlCommand returnCommand)
            {
                _sqlConnection.Setup(x => x.CreateCommand(commandSql)).Returns(returnCommand);
            }

            public string SelectSwitchSql { get { return string.Format(SqlServerProviderSqlDefinitions.SelectSwitch, TableName); } }
            public string SwitchCountSql { get { return string.Format(SqlServerProviderSqlDefinitions.SwitchCount, TableName); } }
            public string InsertSwitchSql { get { return string.Format(SqlServerProviderSqlDefinitions.InsertSwitch, TableName); } }
            public string SelectAllSwitchesSql { get { return string.Format(SqlServerProviderSqlDefinitions.SelectAllSwitches, TableName); } }
        }
    }
}
