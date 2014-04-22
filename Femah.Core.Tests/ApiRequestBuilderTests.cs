using System;
using System.IO;
using System.Web;
using Femah.Core.Api;
using Moq;
using NUnit.Framework;

namespace Femah.Core.Tests
{
    public class ApiRequestBuilderTests
    {
        public class TheBuildMethod
        {
            [Test]
            public void SetsApiRequestBody_IfHttpContextInputStreamIsNotNull()
            {
                //Arrange
                var testable = new ApiRequestBuilder();

                var httpContextMock = new Mock<HttpContextBase>();
                httpContextMock.Setup(x => x.Request.Url)
                    .Returns(new Uri("http://example.com/femah.axd/api/featureswitches"));
                httpContextMock.SetupGet(x => x.Request.HttpMethod).Returns("GET");

                //Build the request Body in JSON
                const string expectedJsonBody =
                    "{\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"SimpleFeatureSwitch\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}";
                
                var inputStream = new MemoryStream();
                var streamWriter = new StreamWriter(inputStream);
                streamWriter.Write(expectedJsonBody);
                streamWriter.Flush();

                httpContextMock.SetupGet(x => x.Request.InputStream).Returns(inputStream);

                //Act
                ApiRequest apiRequest = testable.Build(httpContextMock.Object.Request);

                //Assert
                Assert.AreEqual(expectedJsonBody, apiRequest.Body);
            }
        }
    }
}