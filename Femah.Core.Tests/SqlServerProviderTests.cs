using Femah.Core.Providers;
using Femah.Core.Tests.SqlProviderFakes;
using Moq;
using NUnit.Framework;
using Shouldly;
using System.Collections.Generic;
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
            private const string TableName = "femahSwitches";
            private readonly string[] _featureNames = {"Feature1", "Feature2", "Feature3"};
            private const string ConnectionString = "Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;";
            
            [SetUp]
            public void Init()
            {
                _connectionFactory = new Mock<ISqlConnectionFactory>();
                _sut = new SqlServerProvider(_connectionFactory.Object);
            }

            [Test]
            public void WhenSuppliedWithFeatureNames_AndIfDataStoreIsInitiallyEmpty_SetsUpAllNewFeatureSwitchesInDataStore()
            {
                var connectionFake = new SqlConnectionFake(TableName) {Features = new List<string>()};
                _connectionFactory.Setup(x => x.CreateConnection(ConnectionString)).Returns(connectionFake);

                _sut.Configure(ConnectionString);
                _sut.Initialise(_featureNames);

                _featureNames.Count().ShouldBe(connectionFake.Features.Count);
                _featureNames.ShouldContain(x => connectionFake.Features.Contains(x));
            }

            [Test]
            public void WhenSuppliedWithFeatureNames_AndIfDataStoreContainsDifferentFeatureNames_TheNewFeaturesAreSetup_AndOldFeaturesRemoved()
            {
                var oldFeatureNames = new List<string> {"OldFeature1", "OldFeature2"};
                var connectionFake = new SqlConnectionFake(TableName) { Features = oldFeatureNames };
                _connectionFactory.Setup(x => x.CreateConnection(ConnectionString)).Returns(connectionFake);
                
                _sut.Configure(ConnectionString);
                _sut.Initialise(_featureNames);

                _featureNames.Count().ShouldBe(connectionFake.Features.Count);
                _featureNames.ShouldContain(x => connectionFake.Features.Contains(x));
            }

            // TODO: Calling initialize without setting connection string throws
            // TODO: Sets up passed in switches
            // TODO: if switch does not exist already, simple switch is created
            // TODO: if switch does exist, it is updated
            
            // TODO: Passing in feature names with same existing switches, keeps same switches
            // TODO: Passing in feature names with same and different existing switches, keeps same switches, removes others

            public string SelectSwitchSql { get { return SqlServerProviderSqlDefinitions.CreateSelectSwitchSql(TableName); } }
            public string SwitchCountSql { get { return SqlServerProviderSqlDefinitions.CreateSwitchCountSql(TableName); } }
            public string InsertSwitchSql { get { return SqlServerProviderSqlDefinitions.CreateInsertSwitchSql(TableName); } }
            public string SelectAllSwitchesSql { get { return SqlServerProviderSqlDefinitions.CreateSelectAllSwitchesSql(TableName); } }
            public string DeleteSwitchSql { get { return SqlServerProviderSqlDefinitions.CreateDeleteSwitchSql(TableName); } }
        }
    }
}
