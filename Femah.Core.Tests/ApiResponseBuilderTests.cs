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
            private const string ValidFeatureType = "Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";

            [Test]
            public void ReturnsHttpStatusCodeTo200AndDesiredFeatureSwitchState_IfPutRequestIncludesFeatureSwitchChangesToIsEnabledState()
            {
                //Arrange
                string jsonRequestAndResponse = string.Format(
                        "{{\"IsEnabled\":false,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"{0}\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}}",
                       ValidFeatureType);

                var currentFeatureSwitchState = BuildFeatureSwitch();
                var desiredFeatureSwitchState = BuildFeatureSwitch(false);

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
                var featureSwitch = BuildFeatureSwitch();

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

            private static SimpleFeatureSwitch BuildFeatureSwitch(bool enabled = true)
            {
                return new SimpleFeatureSwitch
                {
                    Name = "TestFeatureSwitch1",
                    IsEnabled = enabled,
                    FeatureType = ValidFeatureType,
                };
            }
        }

        public enum FeatureSwitches
        {
            SomeNewFeature = 1
        }
    }
}