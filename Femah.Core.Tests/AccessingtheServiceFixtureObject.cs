using System;
using System.Web;
using Moq;

namespace Femah.Core.Tests
{
    internal class AccessingtheServiceFixtureObject
    {
        public Mock<HttpContextBase> Context { get; set; }
        public Mock<HttpResponseBase> Response { get; set; }

        public AccessingtheServiceFixtureObject(Uri contextRequestUrl)
        {
            Context = new Mock<HttpContextBase>();
            Context.Setup(x => x.Request.Url)
                .Returns(contextRequestUrl);

            Response = new Mock<HttpResponseBase>();
            Context.Setup(x => x.Response).Returns(Response.Object);
        }
    }
}