using Femah.Core.Api;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Net;
using System.Web;

namespace Femah.Core.Tests
{
    public class ApiRequestBuilderTests
    {
        public class TheBuildMethod
        {
            private Mock<HttpContextBase> _httpContextMock;
            private ApiRequestBuilder _sut;

            [SetUp]
            public void Init()
            {
                _httpContextMock = new Mock<HttpContextBase>();
                _sut = new ApiRequestBuilder();
            }

            [Test]
            public void SetsHttpStatusCodeTo500AndProvidesAccurateErrorMessage_IfRequestUrlIsInvalidAndContainsTooFewSegments()
            {
                const string url = "http://example.com/femah.axd/api";
                SetupContextMock(url, "GET");
                var expectedJsonBody = string.Format("Error: The requested Url '{0}' does not match the expected format /femah.axd/api/[service]/[parameter].", url);

                ApiRequest apiRequest = _sut.Build(_httpContextMock.Object.Request);

                Assert.AreEqual(HttpStatusCode.InternalServerError, apiRequest.ErrorMessageHttpStatusCode);
                Assert.AreEqual(expectedJsonBody, apiRequest.ErrorMessage);
            }

            [Test]
            public void SetsHttpStatusCodeTo500AndProvidesAccurateErrorMessage_IfRequestUrlIsInvalidAndContainsTooManySegments()
            {
                const string url = "http://example.com/femah.axd/api/invalid/url/structure"; 
                SetupContextMock(url, "GET");
                var expectedJsonBody = string.Format("Error: The requested Url '{0}' does not match the expected format /femah.axd/api/[service]/[parameter].", url);

                ApiRequest apiRequest = _sut.Build(_httpContextMock.Object.Request);

                Assert.AreEqual(HttpStatusCode.InternalServerError, apiRequest.ErrorMessageHttpStatusCode);
                Assert.AreEqual(expectedJsonBody, apiRequest.ErrorMessage);
            }
            
            [Test]
            public void SetsHttpStatusCodeTo415AndProvidesAccurateErrorMessage_IfContentTypeIsNotSetToApplicationJsonAndRequestIsAPut()
            {
                SetupContextMock("http://example.com/femah.axd/api/featureswitches/TestFeatureSwitch1", "PUT");
                _httpContextMock.SetupGet(x => x.Request.ContentType).Returns("incorrect/contenttype");
                const string expectedJsonBody = "Error: Content-Type 'incorrect/contenttype' of request is not supported, expecting 'application/json'.";

                ApiRequest apiRequest = _sut.Build(_httpContextMock.Object.Request);

                Assert.AreEqual(HttpStatusCode.UnsupportedMediaType, apiRequest.ErrorMessageHttpStatusCode);
                Assert.AreEqual(expectedJsonBody, apiRequest.ErrorMessage);
            }

            [Test]
            public void SetsApiRequestBody_IfHttpContextInputStreamIsNotNull()
            {
                SetupContextMock("http://example.com/femah.axd/api/featureswitches", "GET");
                const string expectedJsonBody =
                    "{\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"SimpleFeatureSwitch\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}";
                
                var inputStream = CreateInputStream(expectedJsonBody);
                _httpContextMock.SetupGet(x => x.Request.InputStream).Returns(inputStream);

                ApiRequest apiRequest = _sut.Build(_httpContextMock.Object.Request);

                Assert.AreEqual(expectedJsonBody, apiRequest.Body);
            }

            private static MemoryStream CreateInputStream(string expectedJsonBody)
            {
                var inputStream = new MemoryStream();
                var streamWriter = new StreamWriter(inputStream);
                streamWriter.Write(expectedJsonBody);
                streamWriter.Flush();
                return inputStream;
            }

            private void SetupContextMock(string url, string requestType)
            {
                _httpContextMock.Setup(x => x.Request.Url).Returns(new Uri(url));
                _httpContextMock.SetupGet(x => x.Request.HttpMethod).Returns(requestType);
            }
        }
    }
}