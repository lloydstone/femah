using System;
using System.Collections.Generic;
using Femah.Core.FeatureSwitchTypes;
using Moq;
using NUnit.Framework;
using Shouldly;
using Newtonsoft.Json;

namespace Femah.Core.Tests
{
    public class TestObject
    {
        public string Name { get; set; }
        public List<String> Children { get; set; }
    }

    public class FemahTests
    {
        private readonly TestObject _testObject = new TestObject();
        private readonly FemahApiTests _femahApiTests = new FemahApiTests();

        public enum FeatureSwitches
        {
            SomeNewFeature = 1
        }

        #region Exception Handling Tests
        
        [Test]
        public void ExceptionsThrownByProviderAreSwallowed()
        {
            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get(It.IsAny<string>()))
                .Throws(new Exception("Exception thrown by provider."));

            Femah.Configure()
                .Provider(providerMock.Object)
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Initialise();

            Femah.IsFeatureOn((int)FeatureSwitches.SomeNewFeature);            
        }

        [Test]
        public void ExceptionsThrownByContextFactoryAreSwallowed()
        {

            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get(It.IsAny<string>()))
                .Returns(new SimpleFeatureSwitch { IsEnabled = true });

            var contextFactoryMock = new Mock<IFemahContextFactory>();
            contextFactoryMock.Setup(f => f.GenerateContext())
                .Throws(new Exception("Exception thrown by context factory."));

            Femah.Configure()
                .Provider(providerMock.Object)
                .ContextFactory(contextFactoryMock.Object)
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Initialise();

            Femah.IsFeatureOn((int)FeatureSwitches.SomeNewFeature);
        }

        [Test]
        public void ExceptionsThrownByContextAreSwallowed()
        {
            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get(It.IsAny<string>()))
                .Returns( new SimpleFeatureSwitch { IsEnabled = true });

            var contextMock = new Mock<IFemahContext>();
            contextMock.SetupGet(c => c.HttpContext)
                .Throws(new Exception("Exception thrown by context."));

            var contextFactoryMock = new Mock<IFemahContextFactory>();
            contextFactoryMock.Setup(f => f.GenerateContext())
                .Returns(contextMock.Object);

            Femah.Configure()
                .Provider(providerMock.Object)
                .ContextFactory(contextFactoryMock.Object)
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Initialise();

            Femah.IsFeatureOn((int)FeatureSwitches.SomeNewFeature);
        }

        [Test]
        public void ExceptionsThrownByFeatureSwitchesAreSwallowed()
        {
            var featureSwitchMock = new Mock<IFeatureSwitch>();
            featureSwitchMock.Setup(fs => fs.IsOn(It.IsAny<IFemahContext>()))
                .Throws(new Exception("Exception thrown by feature switch."));
            featureSwitchMock.SetupGet(fs => fs.IsEnabled).Returns(true);

            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get(It.IsAny<string>()))
                .Returns(featureSwitchMock.Object);

            Femah.Configure()
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Provider(providerMock.Object)
                .Initialise();

            Femah.IsFeatureOn((int)FeatureSwitches.SomeNewFeature);
        }

        [Test]
        public void InvokingWithInvalidFeatureSwitchIdDoesntThrowException()
        {
            Femah.Configure()
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Initialise();

            var result = Femah.IsFeatureOn(100);

            result.ShouldBe(false);
        }

        #endregion

        #region FeatureSwitch Equality tests

        [Test]
        public void TwoSimpleFeatureSwitchesAreEqualIfAllPropertyValuesAreIdentical()
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
        public void TwoPercentageFeatureSwitchesAreEqualIfAllPropertyValuesAreIdentical()
        {
            //Arrange
            const string validFeatureType = "Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";
            var customAttributes1 = new Dictionary<string, string> {{"percentage", "50"}};
            var customAttributes2 = new Dictionary<string, string> {{"percentage", "50"}};


            var featureSwitch1 = new PercentageFeatureSwitch
            {
                Name = "TestFeatureSwitch1",
                FeatureType = validFeatureType,
                IsEnabled = true
            };
            featureSwitch1.SetCustomAttributes(customAttributes1);

            var featureSwitch2 = new PercentageFeatureSwitch
            {
                Name = "TestFeatureSwitch1",
                FeatureType = validFeatureType,
                IsEnabled = true
            };
            featureSwitch2.SetCustomAttributes(customAttributes2);

            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get("TestFeatureSwitch1"))
                .Returns(featureSwitch1);

            Femah.Configure()
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Provider(providerMock.Object)
                .Initialise();

            //Act
            var equal = featureSwitch1.Equals(featureSwitch2);

            //Assert
            Assert.IsTrue(equal);
        }

        [Test]
        public void TwoSimpleFeatureSwitchesAreNotEqualIfNamesAreDifferent()
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
        public void TwoFeatureSwitchesAreNotEqualIfFeatureTypesAreDifferent()
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
        public void TwoFeatureSwitchesAreNotEqualIfIsEnabledStatesAreDifferent()
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

        //TODO: Review to determine if we're mocking properly.
        [Test]
        public void TwoFeatureSwitchesAreNotEqualIfCustomAttributesAreDifferent()
        {
            //Arrange
            const string validFeatureType = "Femah.Core.FeatureSwitchTypes.PercentageFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";

            var featureSwitch1 = new PercentageFeatureSwitch
            {
                Name = "TestFeatureSwitch1",
                FeatureType = validFeatureType,
                IsEnabled = true,
                PercentageOn = 50
            };

            var featureSwitch2 = new PercentageFeatureSwitch
            {
                Name = "TestFeatureSwitch1",
                FeatureType = validFeatureType,
                IsEnabled = true,
                PercentageOn = 75
            };

            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get("TestFeatureSwitch1"))
                .Returns(featureSwitch1);
            providerMock.Setup(p => p.Get("TestFeatureSwitch1"))
                .Returns(featureSwitch2);

            Femah.Configure()
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Provider(providerMock.Object)
                .Initialise();

            //Act
            var equal = featureSwitch1.Equals(featureSwitch2);

            //Assert
            Assert.IsFalse(equal);
        }


        #endregion

        #region Tolerant Json Reader Tests

        [Test]
        public void ReadsValidJsonMatchingDeserialisedType()
        {
            //Arrange
            string json = @"{
                  'Name': 'Some Test Object',
                  'Children': [
                    'First Child',
                    'Second'
                  ]
                }";

            //Act
            var obj = JsonConvert.DeserializeObject<TestObject>(json);

            Assert.AreEqual("Some Test Object", obj.Name);

            //Note.. As we discussed I'd like to validate the serialised/deserialised JSON in a more structured way using something like the below
            //Assert
            //Are we getting valid Json back?
            //                JsonSchema schema = JsonSchema.Parse(@"{
            //                        'description': '',
            //                        'type': 'array',
            //                        'properties': {
            //                            'IsEnabled': {
            //                                'type': 'boolean', 'required':true
            //                            },
            //                            'Name': {
            //                                'type': 'string', 'required':true
            //                            },
            //                            'FeatureType': {
            //                                'type': 'string', 'required':true
            //                            }
            //                        },
            //                        'additionalProperties': false
            //                    }");
            //
            //                JArray switchTypes = JArray.Parse(responseContent);
            //
            //                bool valid = switchTypes.IsValid(schema);
            //                Assert.IsTrue(valid);
        }

        [Test]
        public void ReadsValidJsonWithExtraFieldNotPresentTargetType()
        {
            //Arrange
            string json = @"{
                  'Name': 'Some Test Object',
                  'Name2': 'Annoyingly added field',
                  'Children': [
                    'First Child',
                    'Second'
                  ]
                }";

            //Act
            var obj = JsonConvert.DeserializeObject<TestObject>(json);

            Assert.AreEqual("Some Test Object", obj.Name);
        }


        [Test]
        public void ReadsValidJsonWithMissingFieldFromTargetType()
        {
            //Arrange
            string json = @"{
                  'Children': [
                    'First Child',
                    'Second'
                  ]
                }";

            //Act
            var obj = JsonConvert.DeserializeObject<TestObject>(json);

            Assert.IsNotNull(obj);
        }

        #endregion
    }
}

