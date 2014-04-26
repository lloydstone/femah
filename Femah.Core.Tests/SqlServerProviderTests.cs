using NUnit.Framework;
using Femah.Core.Providers;
using Shouldly;

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
                var sut = new SqlServerProvider();
                sut.Configure(connectionString);
                
                sut.ConnectionString.ShouldBe(connectionString);
            }
        }
    }
}
