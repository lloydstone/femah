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
            private List<Switch> _features;
            private const string ConnectionString = "Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;";
            
            [SetUp]
            public void Init()
            {
                _connectionFactory = new Mock<ISqlConnectionFactory>();
                _sut = new SqlServerProvider(_connectionFactory.Object);
                _features = CreateSimpleFeatures(new[] {"Feature1", "Feature2", "Feature3"});
            }

            [Test]
            public void WhenSuppliedWithFeatureNames_AndIfDataStoreIsInitiallyEmpty_SetsUpAllNewFeatureSwitchesInDataStore()
            {
                var connectionFake = new SqlConnectionFake(TableName) {Features = new List<Switch>()};
                _connectionFactory.Setup(x => x.CreateConnection(ConnectionString)).Returns(connectionFake);

                _sut.Configure(ConnectionString);
                _sut.Initialise(_features.Select(x => x.Name));

                _features.Count().ShouldBe(connectionFake.Features.Count);
                _features.ShouldContain(x => connectionFake.Features.Contains(x, new SwitchComparer()));
            }

            [Test]
            public void WhenSuppliedWithFeatureNames_AndIfDataStoreContainsDifferentFeatureNames_TheNewFeaturesAreSetup_AndOldFeaturesRemoved()
            {
                var oldFeatureNames = CreateSimpleFeatures(new[] {"OldFeature1", "OldFeature2"});
                var connectionFake = new SqlConnectionFake(TableName) { Features = oldFeatureNames };
                _connectionFactory.Setup(x => x.CreateConnection(ConnectionString)).Returns(connectionFake);
                
                _sut.Configure(ConnectionString);
                _sut.Initialise(_features.Select(x => x.Name));

                _features.Count().ShouldBe(connectionFake.Features.Count);
                _features.ShouldContain(x => connectionFake.Features.Contains(x, new SwitchComparer()));
            }

            [Test]
            public void WhenSuppliedWithFeatureNames_AndIfDataStoreContainsSameAndDifferentFeatureNames_TheRequestedFeaturesAreRetained_AndOldFeaturesRemoved()
            {
                var existingFeatureNames = CreateSimpleFeatures(new [] { "OldFeature1", "OldFeature2" });
                existingFeatureNames.AddRange(_features);

                var connectionFake = new SqlConnectionFake(TableName) { Features = existingFeatureNames };
                _connectionFactory.Setup(x => x.CreateConnection(ConnectionString)).Returns(connectionFake);

                _sut.Configure(ConnectionString);
                _sut.Initialise(_features.Select(x => x.Name));

                _features.Count().ShouldBe(connectionFake.Features.Count);
                _features.ShouldContain(x => connectionFake.Features.Contains(x, new SwitchComparer()));
            }

            private static List<Switch> CreateSimpleFeatures(IEnumerable<string> names)
            {
                return names.Select(Switch.CreateSimpleSwitch).ToList();
            }

            public string SelectSwitchSql { get { return SqlServerProviderSqlDefinitions.CreateSelectSwitchSql(TableName); } }
            public string SwitchCountSql { get { return SqlServerProviderSqlDefinitions.CreateSwitchCountSql(TableName); } }
            public string InsertSwitchSql { get { return SqlServerProviderSqlDefinitions.CreateInsertSwitchSql(TableName); } }
            public string SelectAllSwitchesSql { get { return SqlServerProviderSqlDefinitions.CreateSelectAllSwitchesSql(TableName); } }
            public string DeleteSwitchSql { get { return SqlServerProviderSqlDefinitions.CreateDeleteSwitchSql(TableName); } }
        }
    }
}
