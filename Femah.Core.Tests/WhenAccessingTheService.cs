using Femah.Core.FeatureSwitchTypes;
using Moq;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;

namespace Femah.Core.Tests
{
    public class WhenAccessingTheService
    {
        [Test]
        public void ThenResponseHasCorrectEncodingAndContentType()
        {
            //Arrange
            var testable = new TestableFemahApiHttpHandler();
            var fixtures = new AccessingtheServiceFixtureObject(new Uri("http://example.com/femah.axd/api/featureswitchtypes"));
            fixtures.Response.SetupProperty(x => x.ContentType);
            fixtures.Response.SetupProperty(x => x.ContentEncoding);
            
            //Act
            testable.ProcessRequest(fixtures.Context.Object);

            fixtures.Response.Object.ContentType.ShouldBe("application/json");
            fixtures.Response.Object.ContentEncoding.ShouldBe(Encoding.UTF8);
        }

        [Test]
        public void AndServiceDoesNotSupportParameterQuerying_ThenGetReturns405AndAccurateErrorMessage()
        {
            var testable = new TestableFemahApiHttpHandler();
            var fixtures = new AccessingtheServiceFixtureObject(new Uri("http://example.com/femah.axd/api/featureswitchtypes/simplefeatureswitch"));
            fixtures.Context.SetupGet(x => x.Request.HttpMethod).Returns("GET");
            fixtures.Response.SetupProperty(x => x.StatusCode);

            var featureSwitchTypes = new[] {typeof (SimpleFeatureSwitch), typeof (SimpleFeatureSwitch)};

            const string expectedJsonResponse = "\"Error: Service 'featureswitchtypes' does not support parameter querying.\"";

            Femah.Configure()
                .WithSelectedFeatureSwitchTypes(featureSwitchTypes)
                .Initialise();

            //Get the JSON response by intercepting the call to context.Response.Write
            var responseContent = string.Empty;
            fixtures.Response.Setup(x => x.Write(It.IsAny<string>())).Callback((string r) => { responseContent = r; });

            //Act
            testable.ProcessRequest(fixtures.Context.Object);

            //Asert
            fixtures.Response.Object.StatusCode.ShouldBe(405);
            responseContent.ShouldBe(expectedJsonResponse);
        }

        [Test]
        public void AndTheServiceIsNotFound_ThenGetReturns405()
        {
            //Arrange
            var testable = new TestableFemahApiHttpHandler();
            var fixtures = new AccessingtheServiceFixtureObject(new Uri("http://example.com/femah.axd/api/unknownservicebla"));
            fixtures.Context.SetupGet(x => x.Request.HttpMethod).Returns("GET");
            fixtures.Response.SetupProperty(x => x.StatusCode);

            //Act
            testable.ProcessRequest(fixtures.Context.Object);

            fixtures.Response.Object.StatusCode.ShouldBe(405);
        }

        [Test]
        public void AndTheServiceExists_ThenGetReturns200()
        {
            //Arrange
            var testable = new TestableFemahApiHttpHandler();
            var fixtures = new AccessingtheServiceFixtureObject(new Uri("http://example.com/femah.axd/api/featureswitch"));
            fixtures.Context.SetupGet(x => x.Request.HttpMethod).Returns("GET");
            fixtures.Response.SetupProperty(x => x.StatusCode);
            
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
            testable.ProcessRequest(fixtures.Context.Object);

            //Assert
            fixtures.Response.Object.StatusCode.ShouldBe(200);
        }
    }
}