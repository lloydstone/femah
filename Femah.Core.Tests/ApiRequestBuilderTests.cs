using System;
using System.IO;
using System.Web;
using Femah.Core.Api;
using Moq;
using NUnit.Framework;

namespace Femah.Core.Tests
{
    using System.Net;

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
            public void SetsHttpStatusCodeTo500AndProvidesAccurateErrorMessage_IfRequestUrlIsInvalidAndContainsTooManySegments()
            {
                //Arrange
                var testable = new ApiRequestBuilder();

                var httpContextMock = new Mock<HttpContextBase>();
                httpContextMock.Setup(x => x.Request.Url)
                    .Returns(new Uri("http://example.com/femah.axd/api/invalid/url/structure"));
                httpContextMock.SetupGet(x => x.Request.HttpMethod).Returns("GET");

                const string expectedJsonBody = "Error: The requested Url 'http://example.com/femah.axd/api/invalid/url/structure' does not match the expected format /femah.axd/api/[service]/[parameter].";

                //Act
                ApiRequest apiRequest = testable.Build(httpContextMock.Object.Request);

                //Asert
                Assert.AreEqual(HttpStatusCode.InternalServerError, apiRequest.ErrorMessageHttpStatusCode);
                Assert.AreEqual(expectedJsonBody, apiRequest.ErrorMessage);
            }

            [Test]
            public void SetsHttpStatusCodeTo415AndProvidesAccurateErrorMessage_IfContentTypeIsNotSetToApplicationJsonAndRequestIsAPut()
            {
                //Arrange
                _httpContextMock.Setup(x => x.Request.Url)
                    .Returns(new Uri("http://example.com/femah.axd/api/featureswitches/TestFeatureSwitch1"));
                _httpContextMock.SetupGet(x => x.Request.HttpMethod).Returns("PUT");
                _httpContextMock.SetupGet(x => x.Request.ContentType).Returns("incorrect/contenttype");

                const string expectedJsonBody = "Error: Content-Type 'incorrect/contenttype' of request is not supported, expecting 'application/json'.";

                //Act
                ApiRequest apiRequest = _sut.Build(_httpContextMock.Object.Request);

                //Asert
                Assert.AreEqual(HttpStatusCode.UnsupportedMediaType, apiRequest.ErrorMessageHttpStatusCode);
                Assert.AreEqual(expectedJsonBody, apiRequest.ErrorMessage);
            }

            [Test]
            public void SetsApiRequestBody_IfHttpContextInputStreamIsNotNull()
            {
                //Arrange
                _httpContextMock.Setup(x => x.Request.Url)
                    .Returns(new Uri("http://example.com/femah.axd/api/featureswitches"));
                _httpContextMock.SetupGet(x => x.Request.HttpMethod).Returns("GET");

                //Build the request Body in JSON
                const string expectedJsonBody =
                    "{\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"SimpleFeatureSwitch\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}";
                
                var inputStream = CreateInputStream(expectedJsonBody);

                _httpContextMock.SetupGet(x => x.Request.InputStream).Returns(inputStream);

                //Act
                ApiRequest apiRequest = _sut.Build(_httpContextMock.Object.Request);

                //Assert
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
        }
    }
}