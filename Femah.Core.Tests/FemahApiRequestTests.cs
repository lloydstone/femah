using System;
using System.Text;
using System.Web;
using Moq;
using NUnit.Framework;
using Shouldly;
using System.Collections.Generic;
using Femah.Core.FeatureSwitchTypes;

namespace Femah.Core.Tests
{
    public class FemahApiRequestTests
    {
        public class TheProcessRequestMethod
        {
            private Mock<HttpContextBase> _contextBase;
            private Mock<HttpResponseBase> _response;

            [SetUp]
            public void Init()
            {
                _contextBase = new Mock<HttpContextBase>();
                _response = new Mock<HttpResponseBase>();
            }

            [Test]
            public void Returns200_IfTheServiceExists()
            {
                var testable = new TestableFemahApiHttpHandler();

                SetupContextBaseMock("http://example.com/femah.axd/api/featureswitch");

                _response.SetupProperty(x => x.StatusCode);
                _contextBase.Setup(x => x.Response).Returns(_response.Object);

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
                    .FeatureSwitchEnum(typeof(FemahApiTests.FeatureSwitches))
                    .Provider(providerMock.Object)
                    .Initialise();

                //Act
                testable.ProcessRequest(_contextBase.Object);

                //Assert
                _response.Object.StatusCode.ShouldBe(200);
            }

            [Test]
            public void Returns405_IfServiceIsNotFound()
            {
                //Arrange
                var testable = new TestableFemahApiHttpHandler();
                SetupContextBaseMock("http://example.com/femah.axd/api/unknownservicebla");

                _response.SetupProperty(x => x.StatusCode);
                _contextBase.Setup(x => x.Response).Returns(_response.Object);

                //Act
                testable.ProcessRequest(_contextBase.Object);

                _response.Object.StatusCode.ShouldBe(405);
            }

            [Test]
            public void ReturnsCorrectEncodingAndContentType()
            {
                //Arrange
                var testable = new TestableFemahApiHttpHandler();
                SetupContextBaseMock("http://example.com/femah.axd/api/featureswitchtypes");
                _response.SetupProperty(x => x.ContentType);
                _response.SetupProperty(x => x.ContentEncoding);
                _contextBase.Setup(x => x.Response).Returns(_response.Object);

                //Act
                testable.ProcessRequest(_contextBase.Object);

                _response.Object.ContentType.ShouldBe("application/json");
                _response.Object.ContentEncoding.ShouldBe(Encoding.UTF8);
            }

            [Test]
            public void Returns405AndAccurateErrorMessage_IfServiceDoesntSupportParameterQuerying()
            {
                var testable = new TestableFemahApiHttpHandler();
                SetupContextBaseMock("http://example.com/femah.axd/api/featureswitchtypes/simplefeatureswitch");

                _response.SetupProperty(x => x.StatusCode);
                _contextBase.Setup(x => x.Response).Returns(_response.Object);

                var featureSwitchTypes = new[] { typeof(SimpleFeatureSwitch), typeof(SimpleFeatureSwitch) };

                const string expectedJsonResponse = "\"Error: Service 'featureswitchtypes' does not support parameter querying.\"";

                Femah.Configure()
                    .WithSelectedFeatureSwitchTypes(featureSwitchTypes)
                    .Initialise();

                //Get the JSON response by intercepting the call to context.Response.Write
                var responseContent = string.Empty;
                _response.Setup(x => x.Write(It.IsAny<string>())).Callback((string r) => { responseContent = r; });

                //Act
                testable.ProcessRequest(_contextBase.Object);

                //Asert
                _response.Object.StatusCode.ShouldBe(405);
                responseContent.ShouldBe(expectedJsonResponse);
            }

            private void SetupContextBaseMock(string uriString)
            {
                _contextBase.Setup(x => x.Request.Url).Returns(new Uri(uriString));
                _contextBase.SetupGet(x => x.Request.HttpMethod).Returns("GET");
            }
        }
    }
}