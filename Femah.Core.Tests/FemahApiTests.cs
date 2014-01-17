using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Web;
using Femah.Core.Api;
using Femah.Core.FeatureSwitchTypes;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
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

        [Test]
        public void ApiResponseSetsCorrectContentTypeAndEncoding()
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
        public void ApiResponseReturns500IfCollectionInvalid()
        {
            //Arrange
            var testable = new TestableFemahApiHttpHandler();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.Url)
                .Returns(new Uri("http://example.com/femah.axd/api/unknowncollectionbla"));
            var response = new Mock<HttpResponseBase>();
            response.SetupProperty(x => x.StatusCode);
            context.Setup(x => x.Response).Returns(response.Object);

            //Act
            testable.ProcessRequest(context.Object);

            Assert.AreEqual(500, response.Object.StatusCode);
        }

        [Test]
        public void ApiGetResponseReturns200IfCollectionValid()
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
                providerMock.Setup(p => p.All())
                    .Returns(featureSwitches);

                Femah.Configure()
                    .FeatureSwitchEnum(typeof(FeatureSwitches))
                    .Provider(providerMock.Object)
                    .Initialise();
            
                //Get the JSON response by intercepting the call to context.Response.Write
                string responseContent = string.Empty;
                response.Setup(x => x.Write(It.IsAny<string>())).Callback((string r) => { responseContent = r; });

            //Act
                testable.ProcessRequest(httpContextMock.Object);

            //Assert
                Assert.AreEqual(200, response.Object.StatusCode);
        }

        [Test]
        public void ApiGetResponseReturnsValidCollectionOfFeatureSwitches()
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
            providerMock.Setup(p => p.All())
                .Returns(featureSwitches);

            Femah.Configure()
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Provider(providerMock.Object)
                .Initialise();


            //Get the JSON response by intercepting the call to context.Response.Write
            string responseContent = string.Empty;
            response.Setup(x => x.Write(It.IsAny<string>())).Callback((string r) => { responseContent = r; });

            //Act
            testable.ProcessRequest(httpContextMock.Object);

            //Assert
                Assert.AreEqual(200, response.Object.StatusCode);

                //Are we getting valid Json back?
                JsonSchema schema = JsonSchema.Parse(@"{
                        'description': '',
                        'type': 'array',
                        'properties': {
                            'IsEnabled': {
                                'type': 'boolean', 'required':true
                            },
                            'Name': {
                                'type': 'string', 'required':true
                            },
                            'FeatureType': {
                                'type': 'string', 'required':true
                            }
                        },
                        'additionalProperties': false
                    }");

                JArray switchTypes = JArray.Parse(responseContent);

                bool valid = switchTypes.IsValid(schema);
                Assert.IsTrue(valid);

        }

        //[Test]
//        public void ApiGetResponseReturns200AndValidCollectionOfFeatureSwitchTypes()
//        {
//            //Arrange
//            var testable = new TestableFemahApiHttpHandler();
//            var context = new Mock<HttpContextBase>();
//            context.Setup(x => x.Request.Url)
//                .Returns(new Uri("http://example.com/femah.axd/api/featureswitchtypes"));
//            context.SetupGet(x => x.Request.HttpMethod).Returns("GET");
//
//            var response = new Mock<HttpResponseBase>();
//            response.SetupProperty(x => x.StatusCode);
//            response.SetupProperty(x => x.Output);
//            context.Setup(x => x.Response).Returns(response.Object);
//
//            var providerMock = new Mock<IFeatureSwitchProvider>();
//            Femah.Configure()
//                .Provider(providerMock.Object)
//                .FeatureSwitchEnum(typeof(FemahTests.FeatureSwitches))
//                .Initialise();
//
//            //Get the SwitchTypes directly from the Femah service and convert them to JSON using our friend JSON.NET 
//            //var switchTypes = Femah.AllSwitchTypes();
//            //var serialisedSwitchTypes = JsonConvert.SerializeObject(switchTypes);
//
//            //Get the JSON response by intercepting the call to context.Response.Write
//            string responseContent = string.Empty;
//            response.Setup(x => x.Write(It.IsAny<string>())).Callback((string r) => { responseContent = r; });
//
//            //Act
//            testable.ProcessRequest(context.Object);
//
//
//            //Assert
//            Assert.AreEqual(200, response.Object.StatusCode);
//
//            JsonSchema schema = JsonSchema.Parse(@"{
//                      'type': 'object',
//                      'properties': {
//                        'name': {'type':'string'},
//                        'hobbies': {'type': 'array'}
//                      }
//                    }");
//
//            JObject featureswitches = JObject.Parse(responseContent);
//
//            bool valid = featureswitches.IsValid(schema);
//            Assert.IsTrue(valid);
//        }
    }
}