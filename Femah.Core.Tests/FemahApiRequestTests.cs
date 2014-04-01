using System;
using System.Text;
using System.Web;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Femah.Core.Tests
{
    public class FemahApiRequestTests
    {
        public class TheProcessRequestMethod
        {
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
        }
    }
}
