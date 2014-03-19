using System.Net;
using Femah.Core.Api;
using Femah.Core.FeatureSwitchTypes;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Femah.Core.Tests
{
    public class WhenProcessingPutRequests
    {
        [Test]
        public void GivenRequestContainsInvalidFeatureSwitchType_ThenHttp400AndGenericErrorMessageBodyIsReturned()
        {
            //Arrange
            const string invalidFeatureType = "Invalid.FeatureType.Will.Not.Deserialise";
            var json = string.Format("{{\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"{0}\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}}", invalidFeatureType);
            var apiRequest = new PutApiRequestFactory().WithParameterName("TestFeatureSwitch")
                .WithBody(json).Build();

            const string expectedJsonBody = "\"Error: Unable to deserialise the request body.  Either the JSON is invalid or the supplied 'FeatureType' value is incorrect, have you used the AssemblyQualifiedName as the 'FeatureType' in the request?\"";

            //Act
            var apiResponse = ProcessApiRequest.ProcessPutRequest(apiRequest);

            //Assert
            apiResponse.HttpStatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
            apiResponse.Body.ShouldBe(expectedJsonBody);
        }

        [Test]
        public void GivenRequestNotForFeatureSwitchService_ThenHttp405AndAccurateErrorMessageBodyIsReturned()
        {
            //Arrange
            var apiRequest = new PutApiRequestFactory().ForServiceType(ApiRequest.ApiService.featureswitchtypes)
                .WithParameterName("TestFeatureSwitch").Build();
            const string expectedJsonBody = "\"Error: Service 'featureswitchtypes' does not support parameter querying.\"";

            //Act
            var apiResponse = ProcessApiRequest.ProcessPutRequest(apiRequest);

            //Assert
            Assert.AreEqual((int)HttpStatusCode.MethodNotAllowed, apiResponse.HttpStatusCode);
            Assert.AreEqual(expectedJsonBody, apiResponse.Body);
        }

        [Test]
        public void GivenRequestMissingParameter_ThenHttp405AndAccurateErrorMessageBodyIsReturned()
        {
            //Arrange
            var apiRequest = new PutApiRequestFactory().Build();
            const string expectedJsonBody = "\"Error: Service 'featureswitch' requires a parameter to be passed. Url must match the format /femah.axd/api/[service]/[parameter].\"";

            //Act
            var apiResponse = ProcessApiRequest.ProcessPutRequest(apiRequest);

            //Assert
            apiResponse.HttpStatusCode.ShouldBe((int)HttpStatusCode.MethodNotAllowed);
            apiResponse.Body.ShouldBe(expectedJsonBody);
        }

        [Test]
        public void GivenValidRequest_ThenHttp200AndUpdatedFeatureSwitchBodyIsReturned()
        {
            //Arrange
            const string validFeatureType = "Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";
            var jsonRequestAndResponse = string.Format(
                    "{{\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"{0}\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}}",
                    validFeatureType);

            var apiRequest = new PutApiRequestFactory().WithBody(jsonRequestAndResponse)
                .WithParameterName("TestFeatureSwitch").Build();

            var featureSwitch = new SimpleFeatureSwitch
            {
                Name = "TestFeatureSwitch1",
                IsEnabled = false,
                FeatureType = validFeatureType
            };

            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get("TestFeatureSwitch1"))
                .Returns(featureSwitch);

            Femah.Configure()
                .FeatureSwitchEnum(typeof(FemahApiTests.FeatureSwitches))
                .Provider(providerMock.Object)
                .Initialise();

            //Act
            var apiResponse = ProcessApiRequest.ProcessPutRequest(apiRequest);

            //Assert
            Assert.AreEqual((int)HttpStatusCode.OK, apiResponse.HttpStatusCode);
            Assert.AreEqual(jsonRequestAndResponse, apiResponse.Body);
        }
    }
}