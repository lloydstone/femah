using System;
using System.Collections.Generic;
using System.Web;
using Femah.Core.Api;
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
        public void InvokingWithoutInitialisingDoesntThrowException()
        {
            Femah.IsFeatureOn((int)FeatureSwitches.SomeNewFeature);
        }

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

