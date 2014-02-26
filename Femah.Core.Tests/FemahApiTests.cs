using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Femah.Core.Api;
using Femah.Core.FeatureSwitchTypes;
using Moq;
using NUnit.Framework;

namespace Femah.Core.Tests
{
    public class TestableFemahApiHttpHandler : FemahApiHttpHandler
    {
        public static TestableFemahApiHttpHandler Create()
        {
            return new TestableFemahApiHttpHandler();
        }
    }

    public class FemahApiTests
    {
        public enum FeatureSwitches
        {
            SomeNewFeature = 1
        }

        #region General API (GET methods)

        [Test]
        public void ApiGetSetsCorrectContentTypeAndEncodingInResponse()
        {
            //Arrange
            var testable = new TestableFemahApiHttpHandler();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.Url)
                .Returns(new Uri("http://example.com/femah.axd/api/featureswitchtypes"));
            var response = new Mock<HttpResponseBase>();
            response.SetupProperty(x => x.ContentType);
            response.SetupProperty(x => x.ContentEncoding);
            context.Setup(x => x.Response).Returns(response.Object);

            //Act
            testable.ProcessRequest(context.Object);

            Assert.AreEqual("application/json", response.Object.ContentType);
            Assert.AreEqual(Encoding.UTF8, response.Object.ContentEncoding);
        }

        [Test]
        public void ApiGetReturns200IfServiceExists()
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
                    Name = "TestFeatureSwitch",
                    IsEnabled = false,
                    FeatureType = "SimpleFeatureSwitch"
                })
            };

            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.AllFeatureSwitches())
                .Returns(featureSwitches);

            Femah.Configure()
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Provider(providerMock.Object)
                .Initialise();

            //Act
            testable.ProcessRequest(httpContextMock.Object);

            //Assert
            Assert.AreEqual(200, response.Object.StatusCode);
        }

        [Test]
        public void ApiGetReturns404IfServiceNotFound()
        {
            //Arrange
            var testable = new TestableFemahApiHttpHandler();
            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(x => x.Request.Url)
                .Returns(new Uri("http://example.com/femah.axd/api/unknownservicebla"));
            httpContextMock.SetupGet(x => x.Request.HttpMethod).Returns("GET");

            var response = new Mock<HttpResponseBase>();
            response.SetupProperty(x => x.StatusCode);
            httpContextMock.Setup(x => x.Response).Returns(response.Object);

            //Act
            testable.ProcessRequest(httpContextMock.Object);

            Assert.AreEqual(404, response.Object.StatusCode);
        }

        [Test]
        public void ApiGetReturns405AndAccurateErrorMessageIfServiceDoesNotSupportParameterQuerying()
        {
            //string.Format("Error: Service: '{0}' does not support parameter querying.", apiRequest.Service))
            //Arrange
            var testable = new TestableFemahApiHttpHandler();
            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(x => x.Request.Url)
                .Returns(new Uri("http://example.com/femah.axd/api/featureswitchtypes/simplefeatureswitch"));
            httpContextMock.SetupGet(x => x.Request.HttpMethod).Returns("GET");

            var response = new Mock<HttpResponseBase>();
            response.SetupProperty(x => x.StatusCode);
            httpContextMock.Setup(x => x.Response).Returns(response.Object);

            const int numberOfSwitchTypes = 2;
            var featureSwitchTypes = new Type[numberOfSwitchTypes];
            featureSwitchTypes[0] = typeof(SimpleFeatureSwitch);
            featureSwitchTypes[1] = typeof(SimpleFeatureSwitch);

            const string expectedJsonResponse = "\"Error: Service 'featureswitchtypes' does not support parameter querying.\"";

            Femah.Configure()
                .WithSelectedFeatureSwitchTypes(featureSwitchTypes)
                .Initialise();

            //Get the JSON response by intercepting the call to context.Response.Write
            string responseContent = string.Empty;
            response.Setup(x => x.Write(It.IsAny<string>())).Callback((string r) => { responseContent = r; });

            //Act
            testable.ProcessRequest(httpContextMock.Object);

            //Asert
            Assert.AreEqual(405, response.Object.StatusCode);
            Assert.AreEqual(expectedJsonResponse, responseContent);
        }

        [Test]
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

            const string expectedJsonResponse =
                "\"Error: The requested Url 'http://example.com/femah.axd/api' does not match the expected format /femah.axd/api/[service]/[parameter].\"";

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
                .Returns(new Uri("http://example.com/femah.axd/api/featureswitch/TestFeatureSwitch1"));
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
                .Returns(new Uri("http://example.com/femah.axd/api/featureswitch/unknownfeatureswitch"));
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