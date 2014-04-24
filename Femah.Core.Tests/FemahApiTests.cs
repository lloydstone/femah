using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Femah.Core.Api;
using Femah.Core.FeatureSwitchTypes;
using Moq;
using NUnit.Framework;

namespace Femah.Core.Tests
{
    public class FemahApiTests
    {
        public enum FeatureSwitches
        {
            SomeNewFeature = 1
        }

        #region ApiResponseBuilder

        [Test]
        public void ApiResponseBuilderSetsHttpStatusCodeTo304IfPutRequestBodyIsValidJsonButFeatureSwitchHasNoChanges()
        {
            //Arrange
            const string validFeatureType = "Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";
            string jsonRequestAndResponse = string.Format(
                    "{{\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"{0}\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}}",
                    validFeatureType);

            var featureSwitch = new SimpleFeatureSwitch
            {
                Name = "TestFeatureSwitch1",
                IsEnabled = true,
                FeatureType = validFeatureType
            };

            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get("TestFeatureSwitch1"))
                .Returns(featureSwitch);

            Femah.Configure()
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Provider(providerMock.Object)
                .Initialise();

            //Act
            ApiResponse apiResponse;
            using (var apiResponseBuilder = new ApiResponseBuilder())
            {
                apiResponse = apiResponseBuilder.CreateWithUpdatedFeatureSwitch(featureSwitch);
            }

            //Assert
            Assert.AreEqual((int)HttpStatusCode.NotModified, apiResponse.HttpStatusCode);
            Assert.AreEqual(string.Empty, apiResponse.Body);
        }

        [Test]
        public void ApiResponseBuilderSetsHttpStatusCodeTo200AndReturnsDesiredFeatureSwitchStateIfPutRequestIncludesFeatureSwitchChangesToIsEnabledState()
        {
            //Arrange
            const string validFeatureType = "Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";
            string jsonRequestAndResponse = string.Format(
                    "{{\"IsEnabled\":false,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"{0}\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}}",
                    validFeatureType);

            var currentFeatureSwitchState = new SimpleFeatureSwitch
            {
                Name = "TestFeatureSwitch1",
                IsEnabled = true,
                FeatureType = validFeatureType,
            };

            var desiredFeatureSwitchState = new SimpleFeatureSwitch
            {
                Name = "TestFeatureSwitch1",
                IsEnabled = false,
                FeatureType = validFeatureType
            };

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
            Assert.AreEqual((int)HttpStatusCode.OK, apiResponse.HttpStatusCode);
            Assert.AreEqual(jsonRequestAndResponse, apiResponse.Body);
        }

        [Test]
        public void ApiResponseBuilderSetsHttpStatusCodeTo200AndReturnsDesiredFeatureSwitchStateIfPutRequestIncludesFeatureSwitchChangesToCustomParameters()
        {
            //Arrange
            const string validFeatureType = "Femah.Core.FeatureSwitchTypes.PercentageFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";
            const string jsonResponse = "{\"PercentageOn\":75,\"Description\":\"A simple feature switch that is on for a set percentage of users. The state of the switch is persisted in the user's cookies.If no cookie exists the state is chosen at random (weighted according to the percentage), and then stored in a cookie.\",\"ConfigurationInstructions\":\"Set PercentageOn to the percentage of users who should see this feature. Eg. 10 means the feature is on for 10% of users.\",\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"Femah.Core.FeatureSwitchTypes.PercentageFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null\"}";
                    
            var currentFeatureSwitchState = new PercentageFeatureSwitch
            {
                Name = "TestFeatureSwitch1",
                FeatureType = validFeatureType,
                IsEnabled = true,
                PercentageOn = 50
            };

            var desiredFeatureSwitchState = new PercentageFeatureSwitch
            {
                Name = "TestFeatureSwitch1",
                FeatureType = validFeatureType,
                IsEnabled = true,
                PercentageOn = 75
            };

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
            Assert.AreEqual((int)HttpStatusCode.OK, apiResponse.HttpStatusCode);
            Assert.AreEqual(jsonResponse, apiResponse.Body);
        }

        [Test]
        [Ignore("Can't currently mock Femah easily, seeing as it's both sealed and the methods we're interested in are internal static, thoughts?")]
        public void AssertApiResponseBuilderCallsUpdateOnFeatureTypeStateIsEnabledStateAndAttributes()
        {
            //Arrange
            const string validFeatureType = "Femah.Core.FeatureSwitchTypes.PercentageFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";
            //const string jsonResponse = "{\"PercentageOn\":75,\"Description\":\"A simple feature switch that is on for a set percentage of users. The state of the switch is persisted in the user's cookies.If no cookie exists the state is chosen at random (weighted according to the percentage), and then stored in a cookie.\",\"ConfigurationInstructions\":\"Set PercentageOn to the percentage of users who should see this feature. Eg. 10 means the feature is on for 10% of users.\",\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"Femah.Core.FeatureSwitchTypes.PercentageFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null\"}";

            var currentFeatureSwitchState = new PercentageFeatureSwitch
            {
                Name = "TestFeatureSwitch1",
                FeatureType = validFeatureType,
                IsEnabled = true,
                PercentageOn = 50
            };

            var desiredFeatureSwitchState = new PercentageFeatureSwitch
            {
                Name = "TestFeatureSwitch1",
                FeatureType = validFeatureType,
                IsEnabled = true,
                PercentageOn = 75
            };

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

        #endregion

        #region ProcessApiRequest
        [Test]
        public void ProcessPutRequestSetsHttpStatusCodeTo400AndReturnsGenericErrorMessageInResponseBodyIfPutRequestBodyContainsAJsonArrayOfFeatureSwitches()
        {
            //Arrange
            const string validFeatureType = "Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";

            var apiRequest = new ApiRequest
            {
                HttpMethod = "PUT",
                Service = ApiRequest.ApiService.featureswitches,
                Parameter = "TestFeatureSwitch",
                Body = string.Format("{{\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"{0}\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}},{{\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch2\",\"FeatureType\":\"{0}\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}}", validFeatureType)
            };

            //TODO: I'd like this to be a more specific error with regards to formatting
            const string expectedJsonBody = "\"Error: Unable to deserialise the request body.  Either the JSON is invalid or the supplied 'FeatureType' value is incorrect, have you used the AssemblyQualifiedName as the 'FeatureType' in the request?\"";

            //Act
            ApiResponse apiResponse = ProcessApiRequest.ProcessPutRequest(apiRequest);

            //Assert
            Assert.AreEqual((int)HttpStatusCode.BadRequest, apiResponse.HttpStatusCode);
            Assert.AreEqual(expectedJsonBody, apiResponse.Body);
        }
        
        [Test]
        //[Ignore("We are now testing this in the ApiResponseBuilder(), keep this as an integration test maybe?")]
        public void ProcessPutRequestSetsHttpStatusCodeTo304IfPutRequestBodyIsValidJsonButFeatureSwitchHasNoChanges()
        {
            //Arrange
            const string validFeatureType = "Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";
            string jsonRequestAndResponse = string.Format(
                    "{{\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"{0}\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}}",
                    validFeatureType);

            var apiRequest = new ApiRequest
            {
                HttpMethod = "PUT",
                Service = ApiRequest.ApiService.featureswitches,
                Parameter = "TestFeatureSwitch",
                Body = jsonRequestAndResponse
            };

            var featureSwitch = new SimpleFeatureSwitch
            {
                    Name = "TestFeatureSwitch1",
                    IsEnabled = true,
                    FeatureType = validFeatureType
            };

            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get("TestFeatureSwitch1"))
                .Returns(featureSwitch);

            Femah.Configure()
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Provider(providerMock.Object)
                .Initialise();

            //Act
            ApiResponse apiResponse = ProcessApiRequest.ProcessPutRequest(apiRequest);

            //Assert
            Assert.AreEqual((int)HttpStatusCode.NotModified, apiResponse.HttpStatusCode);
            Assert.AreEqual(string.Empty, apiResponse.Body);
        }


        [Test]
        //[Ignore("We are now testing this in the ApiResponseBuilder(), keep this as an integration test maybe?")]
        public void ProcessPutRequestSetsHttpStatusCodeTo200AndReturnsUpdatedEntity()
        {
            //Arrange
            const string validFeatureType = "Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null";
            string jsonRequestAndResponse = string.Format(
                    "{{\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"{0}\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}}",
                    validFeatureType);

            var apiRequest = new ApiRequest
            {
                HttpMethod = "PUT",
                Service = ApiRequest.ApiService.featureswitches,
                Parameter = "TestFeatureSwitch",
                Body = jsonRequestAndResponse
            };

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
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Provider(providerMock.Object)
                .Initialise();

            //Act
            ApiResponse apiResponse = ProcessApiRequest.ProcessPutRequest(apiRequest);

            //Assert
            Assert.AreEqual((int)HttpStatusCode.OK, apiResponse.HttpStatusCode);
            Assert.AreEqual(jsonRequestAndResponse, apiResponse.Body);
        }
        #endregion

        #region General API (GET methods)

        
        
        [Test]
        [Ignore("We are now testing this in the ApiRequesteBuilder(), keep this as an integration test maybe?")]
        public void ApiGetReturns500AndAccurateErrorMessageIfRequestUrlIsInvalidAndContainsTooManySegments()
        {
            //apiRequest.ErrorMessage = string.Format("Error: The requested Url: '{0}' does not match the expected format /femah.axd/api/[service]/[parameter]", request.Url);
            //Arrange
            var testable = new TestableFemahApiHttpHandler();

            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(x => x.Request.Url)
                .Returns(new Uri("http://example.com/femah.axd/api/invalid/url/structure"));
            httpContextMock.SetupGet(x => x.Request.HttpMethod).Returns("GET");

            var response = new Mock<HttpResponseBase>();
            response.SetupProperty(x => x.StatusCode);
            httpContextMock.Setup(x => x.Response).Returns(response.Object);

            var featureSwitch = new SimpleFeatureSwitch
            {
                Name = "TestFeatureSwitch1",
                IsEnabled = false,
                FeatureType = "SimpleFeatureSwitch"
            };

            const string expectedJsonResponse =
                "\"Error: The requested Url 'http://example.com/femah.axd/api/invalid/url/structure' does not match the expected format /femah.axd/api/[service]/[parameter].\"";

            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get(featureSwitch.Name.ToLower()))
                .Returns(featureSwitch);

            Femah.Configure()
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Provider(providerMock.Object)
                .Initialise();

            //Get the JSON response by intercepting the call to context.Response.Write
            string responseContent = string.Empty;
            response.Setup(x => x.Write(It.IsAny<string>())).Callback((string r) => { responseContent = r; });

            //Act
            testable.ProcessRequest(httpContextMock.Object);

            //Asert
            Assert.AreEqual(500, response.Object.StatusCode);
            Assert.AreEqual(expectedJsonResponse, responseContent);
        }

        [Test]
        [Ignore("We are now testing this in the ApiRequesteBuilder(), keep this as an integration test maybe?")]
        public void ApiGetReturns500AndAccurateErrorMessageIfRequestUrlIsInvalidAndContainsTooFewSegments()
        {
            //Arrange
            var testable = new TestableFemahApiHttpHandler();

            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(x => x.Request.Url)
                .Returns(new Uri("http://example.com/femah.axd/api"));
            httpContextMock.SetupGet(x => x.Request.HttpMethod).Returns("GET");

            var response = new Mock<HttpResponseBase>();
            response.SetupProperty(x => x.StatusCode);
            httpContextMock.Setup(x => x.Response).Returns(response.Object);

            var featureSwitch = new SimpleFeatureSwitch
            {
                Name = "TestFeatureSwitch1",
                IsEnabled = false,
                FeatureType = "SimpleFeatureSwitch"
            };

            const string expectedJsonResponse = "\"Error: The requested Url 'http://example.com/femah.axd/api' does not match the expected format /femah.axd/api/[service]/[parameter].\"";

            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get(featureSwitch.Name.ToLower()))
                .Returns(featureSwitch);

            Femah.Configure()
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Provider(providerMock.Object)
                .Initialise();

            //Get the JSON response by intercepting the call to context.Response.Write
            string responseContent = string.Empty;
            response.Setup(x => x.Write(It.IsAny<string>())).Callback((string r) => { responseContent = r; });

            //Act
            testable.ProcessRequest(httpContextMock.Object);

            //Asert
            Assert.AreEqual(500, response.Object.StatusCode);
            Assert.AreEqual(expectedJsonResponse, responseContent);
        }

        #endregion

        #region FeatureSwitch Service (GET methods)

        [Test]
        public void ApiGetReturns200AndValidJsonArrayOfAllInitialisedFeatureSwitchesFromFeatureSwitchService()
        {
            //Arrange
            var testable = new TestableFemahApiHttpHandler();

            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(x => x.Request.Url)
                .Returns(new Uri("http://example.com/femah.axd/api/featureswitches"));
            httpContextMock.SetupGet(x => x.Request.HttpMethod).Returns("GET");

            var response = new Mock<HttpResponseBase>();
            response.SetupProperty(x => x.StatusCode);
            httpContextMock.Setup(x => x.Response).Returns(response.Object);

            var featureSwitches = new List<IFeatureSwitch>
            {
                (new SimpleFeatureSwitch
                {
                    Name = "TestFeatureSwitch1",
                    IsEnabled = false,
                    FeatureType = "SimpleFeatureSwitch"
                }),
                (new SimpleFeatureSwitch
                {
                    Name = "TestFeatureSwitch2",
                    IsEnabled = false,
                    FeatureType = "SimpleFeatureSwitch"
                })
            };

            const string expectedJsonResponse =
                "[{\"IsEnabled\":false,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"SimpleFeatureSwitch\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"},{\"IsEnabled\":false,\"Name\":\"TestFeatureSwitch2\",\"FeatureType\":\"SimpleFeatureSwitch\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}]";

            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.AllFeatureSwitches())
                .Returns(featureSwitches);

            Femah.Configure()
                .FeatureSwitchEnum(typeof (FeatureSwitches))
                .Provider(providerMock.Object)
                .Initialise();

            //Get the JSON response by intercepting the call to context.Response.Write
            string responseContent = string.Empty;
            response.Setup(x => x.Write(It.IsAny<string>())).Callback((string r) => { responseContent = r; });

            //Act
            testable.ProcessRequest(httpContextMock.Object);

            //Asert
            Assert.AreEqual(200, response.Object.StatusCode);
            Assert.AreEqual(expectedJsonResponse, responseContent);

        }

        [Test]
        public void ApiGetReturns200AndValidJsonForSingleFeatureSwitchFoundWithinFeatureSwitchService()
        {
            //Arrange
            var testable = new TestableFemahApiHttpHandler();

            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(x => x.Request.Url)
                .Returns(new Uri("http://example.com/femah.axd/api/featureswitches/TestFeatureSwitch1"));
            httpContextMock.SetupGet(x => x.Request.HttpMethod).Returns("GET");

            var response = new Mock<HttpResponseBase>();
            response.SetupProperty(x => x.StatusCode);
            httpContextMock.Setup(x => x.Response).Returns(response.Object);

            var featureSwitch = new SimpleFeatureSwitch
            {
                Name = "TestFeatureSwitch1",
                IsEnabled = false,
                FeatureType = "SimpleFeatureSwitch"
            };

            const string expectedJsonResponse =
                "[{\"IsEnabled\":false,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"SimpleFeatureSwitch\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}]";

            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get(featureSwitch.Name.ToLower()))
                .Returns(featureSwitch);

            Femah.Configure()
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Provider(providerMock.Object)
                .Initialise();

            //Get the JSON response by intercepting the call to context.Response.Write
            string responseContent = string.Empty;
            response.Setup(x => x.Write(It.IsAny<string>())).Callback((string r) => { responseContent = r; });

            //Act
            testable.ProcessRequest(httpContextMock.Object);

            //Asert
            Assert.AreEqual(200, response.Object.StatusCode);
            Assert.AreEqual(expectedJsonResponse, responseContent);
        }

        [Test]
        public void ApiGetReturns404AndEmptyResponseBodyIfFeatureSwitchNotfoundWithinFeatureSwitchService()
        {
            //Arrange
            var testable = new TestableFemahApiHttpHandler();

            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(x => x.Request.Url)
                .Returns(new Uri("http://example.com/femah.axd/api/featureswitches/unknownfeatureswitch"));
            httpContextMock.SetupGet(x => x.Request.HttpMethod).Returns("GET");

            var response = new Mock<HttpResponseBase>();
            response.SetupProperty(x => x.StatusCode);
            httpContextMock.Setup(x => x.Response).Returns(response.Object);

            var featureSwitch = new SimpleFeatureSwitch
            {
                Name = "TestFeatureSwitch1",
                IsEnabled = false,
                FeatureType = "SimpleFeatureSwitch"
            };

            const string expectedJsonResponse = "\"\"";

            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get(featureSwitch.Name.ToLower()))
                .Returns(featureSwitch);

            Femah.Configure()
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Provider(providerMock.Object)
                .Initialise();

            //Get the JSON response by intercepting the call to context.Response.Write
            string responseContent = string.Empty;
            response.Setup(x => x.Write(It.IsAny<string>())).Callback((string r) => { responseContent = r; });

            //Act
            testable.ProcessRequest(httpContextMock.Object);

            //Asert
            Assert.AreEqual(404, response.Object.StatusCode);
            Assert.AreEqual(expectedJsonResponse, responseContent);

        }

        #endregion

        #region FeatureSwitch Service (PUT methods)

        [Test]
        [Ignore("We are now testing this in the ApiRequestBuilder(), keep this as an integration test maybe?")]
        public void ApiPutReturns415AndAccurateErrorMessageIfContentTypeIsNotSetToApplicationJson()
        {
            //Arrange
            var testable = new TestableFemahApiHttpHandler();

            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(x => x.Request.Url)
                .Returns(new Uri("http://example.com/femah.axd/api/featureswitches/TestFeatureSwitch1"));
            httpContextMock.SetupGet(x => x.Request.HttpMethod).Returns("PUT");
            httpContextMock.SetupGet(x => x.Request.ContentType).Returns("incorrect/contenttype");


            var response = new Mock<HttpResponseBase>();
            response.SetupProperty(x => x.StatusCode);
            httpContextMock.Setup(x => x.Response).Returns(response.Object);

            var featureSwitch = new SimpleFeatureSwitch
            {
                Name = "TestFeatureSwitch1",
                IsEnabled = false,
                FeatureType = "SimpleFeatureSwitch"
            };

            const string expectedJsonResponse = "\"Error: Content-Type 'incorrect/contenttype' of request is not supported, expecting 'application/json'.\"";

            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get(featureSwitch.Name.ToLower()))
                .Returns(featureSwitch);

            Femah.Configure()
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Provider(providerMock.Object)
                .Initialise();

            //Get the JSON response by intercepting the call to context.Response.Write
            string responseContent = string.Empty;
            response.Setup(x => x.Write(It.IsAny<string>())).Callback((string r) => { responseContent = r; });

            //Act
            testable.ProcessRequest(httpContextMock.Object);

            //Asert
            Assert.AreEqual(415, response.Object.StatusCode);
            Assert.AreEqual(expectedJsonResponse, responseContent);
        }

        
        #endregion

        #region FeatureSwitchType Service (GET methods)

        [Test]
        public void ApiGetReturns200AndValidJsonArrayOfAllInitialisedFeatureSwitchTypesFromFeatureSwitchTypeService()
        {
            //Arrange
            var testable = new TestableFemahApiHttpHandler();
            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(x => x.Request.Url)
                .Returns(new Uri("http://example.com/femah.axd/api/featureswitchtypes"));
            httpContextMock.SetupGet(x => x.Request.HttpMethod).Returns("GET");

            var response = new Mock<HttpResponseBase>();
            response.SetupProperty(x => x.StatusCode);
            httpContextMock.Setup(x => x.Response).Returns(response.Object);

            const int numberOfSwitchTypes = 2;
            var featureSwitchTypes = new Type[numberOfSwitchTypes];
            featureSwitchTypes[0] = typeof (SimpleFeatureSwitch);
            featureSwitchTypes[1] = typeof(SimpleFeatureSwitch);

            const string expectedJsonResponse = "[{\"Name\":\"SimpleFeatureSwitch\",\"FeatureSwitchType\":\"Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"},{\"Name\":\"SimpleFeatureSwitch\",\"FeatureSwitchType\":\"Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}]";

            Femah.Configure()
                .WithSelectedFeatureSwitchTypes(featureSwitchTypes)
                .Initialise();

            //Get the JSON response by intercepting the call to context.Response.Write
            string responseContent = string.Empty;
            response.Setup(x => x.Write(It.IsAny<string>())).Callback((string r) => { responseContent = r; });

            //Act
            testable.ProcessRequest(httpContextMock.Object);

            //Assert
            Assert.AreEqual(200, response.Object.StatusCode);
            Assert.AreEqual(expectedJsonResponse, responseContent);

        }

        #endregion
    }
}