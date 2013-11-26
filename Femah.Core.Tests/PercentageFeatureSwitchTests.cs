using System;
using System.Web;
using Femah.Core.FeatureSwitchTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;

namespace Femah.Core.Tests
{
    [TestClass]
    public class PercentageFeatureSwitchTests
    {
        private HttpCookieCollection _cookies;
        private FemahContext _femahContext;

        [TestInitialize]
        public void Initialize()
        {
            // Initialise cookie collection.
            _cookies = new HttpCookieCollection();

            // Mock out the HttpContext - mock uses our local cookie collection.
            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(c => c.Request.Cookies).Returns(_cookies);
            httpContextMock.Setup(c => c.Response.Cookies).Returns(_cookies);

            // Create new FemahContext using mock HttpContext.
            _femahContext = new FemahContext(httpContextMock.Object);
        }

        [TestMethod]
        public void UsesValueFromCookieWhenCookieExists()
        {
           var featureSwitch = new PercentageFeatureSwitch()
            {
                IsEnabled = true,
                Name = "testPercentageFeatureSwitch"
            };

            _cookies.Add(new HttpCookie(featureSwitch.Name, true.ToString()));
            var result = featureSwitch.IsOn(_femahContext);
            result.ShouldBe(true);

            _cookies.Clear();
            _cookies.Add(new HttpCookie(featureSwitch.Name, false.ToString()));
            result = featureSwitch.IsOn(_femahContext);
            result.ShouldBe(false);
        }

        [TestMethod]
        public void SetsCookieWhenNoCookieExists()
        {
            var featureSwitch = new PercentageFeatureSwitch()
            {
                IsEnabled = true,
                Name = "testPercentageFeatureSwitch"
            };

            var result = featureSwitch.IsOn(_femahContext);

            _cookies.Count.ShouldBe(1);
            _cookies[0].Name.ShouldBe(featureSwitch.Name);
            _cookies[0].Value.ShouldBe(result.ToString());
        }

        [TestMethod]
        public void IsOnIffRandomNumberBelowThreshold()
        {
            // Random number below threshold (should be on).
            var featureSwitch = new PercentageFeatureSwitch(() => 0.1)
            {
                PercentageOn = 30,
                Name = "testfeatureswitch",
                IsEnabled = true
            };

            var result = featureSwitch.IsOn(_femahContext);
            result.ShouldBe(true);

            // Random number above threshold (should be off).
            featureSwitch = new PercentageFeatureSwitch(() => 0.4)
            {
                PercentageOn = 30,
                Name = "testfeatureswitch",
                IsEnabled = true
            };

            _cookies.Clear();
            result = featureSwitch.IsOn(_femahContext);
            result.ShouldBe(false);

            // Random number same as threshold (should be off).
            featureSwitch = new PercentageFeatureSwitch(() => 0.1)
            {
                PercentageOn = 10,
                Name = "testfeatureswitch",
                IsEnabled = true
            };

            _cookies.Clear();
            result = featureSwitch.IsOn(_femahContext);
            result.ShouldBe(false);


            // Random number same as threshold, threshold is zero. (Should be off).
            featureSwitch = new PercentageFeatureSwitch(() => 0.0)
            {
                PercentageOn = 0,
                Name = "testfeatureswitch",
                IsEnabled = true
            };

            _cookies.Clear();
            result = featureSwitch.IsOn(_femahContext);
            result.ShouldBe(false);
        }
    }
}
