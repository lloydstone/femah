using System.Net;
using Femah.Core.Api;
using Femah.Core.FeatureSwitchTypes;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Femah.Core.Tests
{
    public class ProcessApiRequestTests
    {
        public class TheProcessPutRequestMethod
        {
            [Test, TestCaseSource(typeof (InvalidRequestTestData), "TestCases")]
            public void ReturnsAppropriateHttpCodeAndErrorMessage_IfRequestHasInvalidData(string parameterName,
                string requestJson, string expectedResponse, HttpStatusCode expectedStatusCode)
            {
                var apiRequest = new PutApiRequestFactory().WithParameterName(parameterName)
                    .WithBody(requestJson).Build();

                var apiResponse = ProcessApiRequest.ProcessPutRequest(apiRequest);

                apiResponse.HttpStatusCode.ShouldBe((int) expectedStatusCode);
                apiResponse.Body.ShouldBe(expectedResponse);
            }

            [Test]
            public void ReturnsHttp405AndBodyWithAccurateErrorMessage_IfRequestIsNotForFeatureSwitchService()
            {
                //Arrange
                var apiRequest = new PutApiRequestFactory().ForServiceType(ApiRequest.ApiService.featureswitchtypes)
                    .WithParameterName("TestFeatureSwitch").Build();
                const string expectedJsonBody =
                    "\"Error: Service 'featureswitchtypes' does not support parameter querying.\"";

                //Act
                var apiResponse = ProcessApiRequest.ProcessPutRequest(apiRequest);

                //Assert
                Assert.AreEqual((int) HttpStatusCode.MethodNotAllowed, apiResponse.HttpStatusCode);
                Assert.AreEqual(expectedJsonBody, apiResponse.Body);
            }

            [Test]
            public void ReturnsHttp405AndBodyWithAccurateErrorMessage_IfRequestIsMissingParameter()
            {
                //Arrange
                var apiRequest = new PutApiRequestFactory().Build();
                const string expectedJsonBody =
                    "\"Error: Service 'featureswitches' requires a parameter to be passed. Url must match the format /femah.axd/api/[service]/[parameter].\"";

                //Act
                var apiResponse = ProcessApiRequest.ProcessPutRequest(apiRequest);

                //Assert
                apiResponse.HttpStatusCode.ShouldBe((int) HttpStatusCode.MethodNotAllowed);
                apiResponse.Body.ShouldBe(expectedJsonBody);
            }

            [Test]
            public void ReturnsHttp200AndBodyWithUpdatedFeatureSwitch_IfRequestIsValid()
            {
                //Arrange
                const string validFeatureType =
                    "Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";
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
                    .FeatureSwitchEnum(typeof (FemahApiTests.FeatureSwitches))
                    .Provider(providerMock.Object)
                    .Initialise();

                //Act
                var apiResponse = ProcessApiRequest.ProcessPutRequest(apiRequest);

                //Assert
                Assert.AreEqual((int) HttpStatusCode.OK, apiResponse.HttpStatusCode);
                Assert.AreEqual(jsonRequestAndResponse, apiResponse.Body);
            }
        }
    }
}