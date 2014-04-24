using Femah.Core.Api;
using Femah.Core.FeatureSwitchTypes;
using Moq;
using NUnit.Framework;
using Shouldly;
using System.Net;

namespace Femah.Core.Tests
{
    public class ApiResponseBuilderTests
    {
        public class TheCreateWithUpdatedFeatureSwitchMethod
        {
            private const string ValidSimpleFeatureType = "Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";
            private const string ValidPercentageFeatureType = "Femah.Core.FeatureSwitchTypes.PercentageFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";

            [Test]
            public void ReturnsHttpStatusCodeTo200AndDesiredFeatureSwitchState_IfPutRequestIncludesFeatureSwitchChangesToCustomParameters()
            {
                //Arrange
                const string jsonResponse = "{\"PercentageOn\":75,\"Description\":\"A simple feature switch that is on for a set percentage of users. The state of the switch is persisted in the user's cookies.If no cookie exists the state is chosen at random (weighted according to the percentage), and then stored in a cookie.\",\"ConfigurationInstructions\":\"Set PercentageOn to the percentage of users who should see this feature. Eg. 10 means the feature is on for 10% of users.\",\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"Femah.Core.FeatureSwitchTypes.PercentageFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null\"}";

                var currentFeatureSwitchState = BuildPercentageFeatureSwitch(50);
                var desiredFeatureSwitchState = BuildPercentageFeatureSwitch(75);

                var providerMock = new Mock<IFeatureSwitchProvider>();
                providerMock.Setup(p => p.Get("TestFeatureSwitch1"))
                    .Returns(currentFeatureSwitchState);

                Femah.Configure()
                    .FeatureSwitchEnum(typeof(FeatureSwitches))
                    .Provider(providerMock.Object)
                    .Initialise();

                //Act
                ApiResponse apiResponse;
                using (var apiResponseBuilder = new ApiResponseBuilder())
                {
                    apiResponse = apiResponseBuilder.CreateWithUpdatedFeatureSwitch(desiredFeatureSwitchState);
                }

                //Assert
                apiResponse.HttpStatusCode.ShouldBe((int)HttpStatusCode.OK);
                apiResponse.Body.ShouldBe(jsonResponse);
        
            }
            
            [Test]
            public void ReturnsHttpStatusCodeTo200AndDesiredFeatureSwitchState_IfPutRequestIncludesFeatureSwitchChangesToIsEnabledState()
            {
                //Arrange
                string jsonRequestAndResponse = string.Format(
                        "{{\"IsEnabled\":false,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"{0}\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}}",
                       ValidSimpleFeatureType);

                var currentFeatureSwitchState = BuildSimpleFeatureSwitch();
                var desiredFeatureSwitchState = BuildSimpleFeatureSwitch(false);

                var providerMock = new Mock<IFeatureSwitchProvider>();
                providerMock.Setup(p => p.Get("TestFeatureSwitch1"))
                    .Returns(currentFeatureSwitchState);

                Femah.Configure()
                    .FeatureSwitchEnum(typeof(FeatureSwitches))
                    .Provider(providerMock.Object)
                    .Initialise();

                //Act
                ApiResponse apiResponse;
                using (var apiResponseBuilder = new ApiResponseBuilder())
                {
                    apiResponse = apiResponseBuilder.CreateWithUpdatedFeatureSwitch(desiredFeatureSwitchState);
                }

                //Assert
                apiResponse.HttpStatusCode.ShouldBe((int)HttpStatusCode.OK);
                apiResponse.Body.ShouldBe(jsonRequestAndResponse);
            }

            [Test]
            public void ReturnsHttpStatusCodeTo304_IfPutRequestBodyIsValidJsonButFeatureSwitchHasNoChanges()
            {
                //Arrange
                var featureSwitch = BuildSimpleFeatureSwitch();

                var providerMock = new Mock<IFeatureSwitchProvider>();
                providerMock.Setup(p => p.Get("TestFeatureSwitch1"))
                    .Returns(featureSwitch);

                Femah.Configure()
                    .FeatureSwitchEnum(typeof(FemahApiTests.FeatureSwitches))
                    .Provider(providerMock.Object)
                    .Initialise();

                //Act
                ApiResponse apiResponse;
                using (var apiResponseBuilder = new ApiResponseBuilder())
                {
                    apiResponse = apiResponseBuilder.CreateWithUpdatedFeatureSwitch(featureSwitch);
                }

                //Assert
                apiResponse.HttpStatusCode.ShouldBe((int)HttpStatusCode.NotModified);
                apiResponse.Body.ShouldBe(string.Empty);
            }

            [Test]
            [Ignore("Can't currently mock Femah easily, seeing as it's both sealed and the methods we're interested in are internal static, thoughts?")]
            public void CallsUpdateOnFeatureTypeStateIsEnabledStateAndAttributes()
            {
                //Arrange
                //const string jsonResponse = "{\"PercentageOn\":75,\"Description\":\"A simple feature switch that is on for a set percentage of users. The state of the switch is persisted in the user's cookies.If no cookie exists the state is chosen at random (weighted according to the percentage), and then stored in a cookie.\",\"ConfigurationInstructions\":\"Set PercentageOn to the percentage of users who should see this feature. Eg. 10 means the feature is on for 10% of users.\",\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"Femah.Core.FeatureSwitchTypes.PercentageFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null\"}";

                var currentFeatureSwitchState = BuildPercentageFeatureSwitch(50);
                var desiredFeatureSwitchState  = BuildPercentageFeatureSwitch(75);
                  
                var providerMock = new Mock<IFeatureSwitchProvider>();
                providerMock.Setup(p => p.Get("TestFeatureSwitch1"))
                    .Returns(currentFeatureSwitchState);

                var femahMock = new Mock<Femah>();
                //femahMock.Setup(f => f)
                //TODO: Can't currently mock Femah easily, seeing as it's both sealed and the methods we're interested in are internal static, thoughts?


                Femah.Configure()
                    .FeatureSwitchEnum(typeof(FeatureSwitches))
                    .Provider(providerMock.Object)
                    .Initialise();

                //Act
                ApiResponse apiResponse;
                using (var apiResponseBuilder = new ApiResponseBuilder())
                {
                    apiResponse = apiResponseBuilder.CreateWithUpdatedFeatureSwitch(desiredFeatureSwitchState);
                }

                //Assert
                //Assert.AreEqual((int)HttpStatusCode.OK, apiResponse.HttpStatusCode);
                //Assert.AreEqual(jsonResponse, apiResponse.Body);

            }

            private static PercentageFeatureSwitch BuildPercentageFeatureSwitch(int percentage, bool enabled = true)
            {
                return new PercentageFeatureSwitch
                {
                    Name = "TestFeatureSwitch1",
                    FeatureType = ValidPercentageFeatureType,
                    IsEnabled = enabled,
                    PercentageOn = percentage
                };
            }

            private static SimpleFeatureSwitch BuildSimpleFeatureSwitch(bool enabled = true)
            {
                return new SimpleFeatureSwitch
                {
                    Name = "TestFeatureSwitch1",
                    IsEnabled = enabled,
                    FeatureType = ValidSimpleFeatureType,
                };
            }
        }

        public enum FeatureSwitches
        {
            SomeNewFeature = 1
        }
    }
}