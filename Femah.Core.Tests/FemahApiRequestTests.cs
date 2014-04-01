using System;
using System.Text;
using System.Web;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Femah.Core.Tests
{
    using FeatureSwitchTypes;

    public class FemahApiRequestTests
    {
        public class TheProcessRequestMethod
        {
            [Test]
            public void Returns405IfServiceIsNotFound()
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

                response.Object.StatusCode.ShouldBe(405);
            }

            [Test]
            public void ReturnsCorrectEncodingAndContentType()
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

                response.Object.ContentType.ShouldBe("application/json");
                response.Object.ContentEncoding.ShouldBe(Encoding.UTF8);
            }

            [Test]
            public void Returns405AndAccurateErrorMessage_IfServiceDoesntSupportParameterQuerying()
            {
                var testable = new TestableFemahApiHttpHandler();
                var httpContextMock = new Mock<HttpContextBase>();
                httpContextMock.Setup(x => x.Request.Url)
                    .Returns(new Uri("http://example.com/femah.axd/api/featureswitchtypes/simplefeatureswitch"));
                httpContextMock.SetupGet(x => x.Request.HttpMethod).Returns("GET");

                var response = new Mock<HttpResponseBase>();
                response.SetupProperty(x => x.StatusCode);
                httpContextMock.Setup(x => x.Response).Returns(response.Object);

                var featureSwitchTypes = new[] { typeof(SimpleFeatureSwitch), typeof(SimpleFeatureSwitch) };

                const string expectedJsonResponse = "\"Error: Service 'featureswitchtypes' does not support parameter querying.\"";

                Femah.Configure()
                    .WithSelectedFeatureSwitchTypes(featureSwitchTypes)
                    .Initialise();

                //Get the JSON response by intercepting the call to context.Response.Write
                var responseContent = string.Empty;
                response.Setup(x => x.Write(It.IsAny<string>())).Callback((string r) => { responseContent = r; });

                //Act
                testable.ProcessRequest(httpContextMock.Object);

                //Asert
                response.Object.StatusCode.ShouldBe(405);
                responseContent.ShouldBe(expectedJsonResponse);
            }
        }
    }
}
