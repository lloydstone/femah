using Femah.Core.FeatureSwitchTypes;
using NUnit.Framework;

namespace Femah.Core.Tests
{
    public class SimpleFeatureSwitchTests
    {
        public class TheEqualsMethod
        {
            public enum FeatureSwitches
            {
                SomeNewFeature = 1
            }

            [Test]
            public void GivenTwoSimpleFeatureSwitches_ReturnsTrue_IfIfAllPropertyValuesAreIdentical()
            {
                //Arrange
                const string validFeatureType = "Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";
                var featureSwitch1 = new SimpleFeatureSwitch
                {
                    Name = "TestFeatureSwitch1",
                    FeatureType = validFeatureType,
                    IsEnabled = true
                };
                var featureSwitch2 = new SimpleFeatureSwitch
                {
                    Name = "TestFeatureSwitch1",
                    FeatureType = validFeatureType,
                    IsEnabled = true
                };

                //Act
                var equal = featureSwitch1.Equals(featureSwitch2);

                //Assert
                Assert.IsTrue(equal);
            }
            
            [Test]
            public void GivenTwoSimpleFeatureSwitches_ReturnsFalse_NamesAreDifferent()
            {
                //Arrange
                const string validFeatureType = "Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";
                var featureSwitch1 = new SimpleFeatureSwitch
                {
                    Name = "TestFeatureSwitch1",
                    FeatureType = validFeatureType
                };
                var featureSwitch2 = new SimpleFeatureSwitch
                {
                    Name = "TestFeatureSwitch2",
                    FeatureType = validFeatureType
                };

                //Act
                var equal = featureSwitch1.Equals(featureSwitch2);

                //Assert
                Assert.IsFalse(equal);
            }

            [Test]
            public void GivenTwoFeatureSwitches_ReturnsFalse_IfFeatureTypesAreDifferent()
            {
                //Arrange
                const string validFeatureType1 = "Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";
                const string validFeatureType2 = "Femah.Core.FeatureSwitchTypes.PercentageFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";
                var featureSwitch1 = new SimpleFeatureSwitch
                {
                    Name = "TestFeatureSwitch1",
                    FeatureType = validFeatureType1
                };
                var featureSwitch2 = new PercentageFeatureSwitch
                {
                    Name = "TestFeatureSwitch1",
                    FeatureType = validFeatureType2
                };

                //Act
                var equal = featureSwitch1.Equals(featureSwitch2);

                //Assert
                Assert.IsFalse(equal);
            }

            [Test]
            public void GivenTwoSimpleFeatureSwitches_ReturnsFalse_IfIsEnabledStatesAreDifferent()
            {
                //Arrange
                const string validFeatureType = "Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";
                var featureSwitch1 = new SimpleFeatureSwitch
                {
                    Name = "TestFeatureSwitch1",
                    FeatureType = validFeatureType,
                    IsEnabled = true
                };
                var featureSwitch2 = new SimpleFeatureSwitch
                {
                    Name = "TestFeatureSwitch1",
                    FeatureType = validFeatureType,
                    IsEnabled = false
                };

                //Act
                var equal = featureSwitch1.Equals(featureSwitch2);

                //Assert
                Assert.IsFalse(equal);
            }
        }
    }
}