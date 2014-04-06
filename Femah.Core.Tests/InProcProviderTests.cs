using Femah.Core.Providers;
using NUnit.Framework;

namespace Femah.Core.Tests
{
    public class InProcProviderTests
    {
        public enum FeatureSwitches
        {
            SomeNewFeature = 1
        }

        public class TheGetMethod
        {
            
            [SetUp]
            public void Initialize()
            {
            }

            [Test]
            public void FeatureTypeIsFullyQualifiedAssemblyName()
            {
                //Arrange
                var inProcProvider = new InProcProvider();
                const string expectedFullyQualifiedFeatureType = "Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";

                //Act
                Femah.Configure()
                    .FeatureSwitchEnum(typeof(FeatureSwitches))
                    .Initialise();

                var featureSwitch = inProcProvider.Get("SomeNewFeature");

                //Assert
                Assert.AreEqual(expectedFullyQualifiedFeatureType, featureSwitch.FeatureType);
                

            }
        }

    }
}
