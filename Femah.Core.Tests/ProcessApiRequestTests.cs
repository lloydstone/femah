﻿using System.Net;
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
            private const string ValidFeatureType = "Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";

            [Test]
            public void ReturnsHttpStatusCodeTo304_IfPutRequestBodyIsValidJsonButFeatureSwitchHasNoChanges()
            {
                //Arrange
                var jsonRequestAndResponse = string.Format(
                        "{{\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"{0}\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}}",
                        ValidFeatureType);

                var apiRequest = new PutApiRequestFactory()
                    .WithParameterName("TestFeatureSwitch")
                    .WithBody(jsonRequestAndResponse).Build();

                var providerMock = BuildSimpleFeatureSwitch(ValidFeatureType, true);

                Femah.Configure()
                    .FeatureSwitchEnum(typeof(FeatureSwitches))
                    .Provider(providerMock.Object)
                    .Initialise();

                //Act
                var apiResponse = ProcessApiRequest.ProcessPutRequest(apiRequest);

                //Assert
                apiResponse.HttpStatusCode.ShouldBe((int)HttpStatusCode.NotModified);
                apiResponse.Body.ShouldBe(string.Empty);
            }

            [Test]
            public void ReturnsGenericErrorMessageInResponseBodyAndSetsHttpStatusCodeTo400_IfPutRequestBodyContainsAJsonArrayOfFeatureSwitches()
            {
                //Arrange
                var requestBody = string.Format("{{\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"{0}\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}},{{\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch2\",\"FeatureType\":\"{0}\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}}", ValidFeatureType);
                const string expectedJsonBody = "\"Error: Unable to deserialise the request body.  Either the JSON is invalid or the supplied 'FeatureType' value is incorrect, have you used the AssemblyQualifiedName as the 'FeatureType' in the request?\"";
                //TODO: I'd like this to be a more specific error with regards to formatting
                
                var apiRequest = new PutApiRequestFactory()
                    .WithParameterName("TestFeatureSwitch")
                    .WithBody(requestBody).Build();

                //Act
                var apiResponse = ProcessApiRequest.ProcessPutRequest(apiRequest);

                //Assert
                apiResponse.HttpStatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
                apiResponse.Body.ShouldBe(expectedJsonBody);
            }
    
            [Test]
            public void ReturnsUpdatedEntityAndSetsHttpStatusCodeTo200_IfRequestIsValid()
            {
                var jsonRequestAndResponse = string.Format(
                        "{{\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"{0}\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}}",
                        ValidFeatureType);

                var apiRequest = new PutApiRequestFactory().WithBody(jsonRequestAndResponse)
                    .WithParameterName("TestFeatureSwitch")
                    .Build();

                var providerMock = BuildSimpleFeatureSwitch(ValidFeatureType);

                Femah.Configure()
                    .FeatureSwitchEnum(typeof(FeatureSwitches))
                    .Provider(providerMock.Object)
                    .Initialise();

                var apiResponse = ProcessApiRequest.ProcessPutRequest(apiRequest);

                apiResponse.HttpStatusCode.ShouldBe((int)HttpStatusCode.OK);
                apiResponse.Body.ShouldBe(jsonRequestAndResponse);
            }
            
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
                apiResponse.HttpStatusCode.ShouldBe((int) HttpStatusCode.MethodNotAllowed);
                apiResponse.Body.ShouldBe(expectedJsonBody);
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
                var jsonRequestAndResponse = string.Format(
                    "{{\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"{0}\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}}",
                    ValidFeatureType);

                var apiRequest = new PutApiRequestFactory().WithBody(jsonRequestAndResponse)
                    .WithParameterName("TestFeatureSwitch").Build();

                var providerMock = BuildSimpleFeatureSwitch(ValidFeatureType);
         
                Femah.Configure()
                    .FeatureSwitchEnum(typeof (FemahApiTests.FeatureSwitches))
                    .Provider(providerMock.Object)
                    .Initialise();

                //Act
                var apiResponse = ProcessApiRequest.ProcessPutRequest(apiRequest);

                //Assert
                apiResponse.HttpStatusCode.ShouldBe((int)HttpStatusCode.OK);
                apiResponse.Body.ShouldBe(jsonRequestAndResponse);
            }

            private static Mock<IFeatureSwitchProvider> BuildSimpleFeatureSwitch(string validFeatureType, bool enabled = false)
            {
                var featureSwitch = new SimpleFeatureSwitch
                {
                    Name = "TestFeatureSwitch1",
                    IsEnabled = enabled,
                    FeatureType = validFeatureType
                };

                var providerMock = new Mock<IFeatureSwitchProvider>();
                providerMock.Setup(p => p.Get("TestFeatureSwitch1"))
                    .Returns(featureSwitch);
                return providerMock;
            }
        }
    }

    public enum FeatureSwitches
    {
        SomeNewFeature = 1
    }
}