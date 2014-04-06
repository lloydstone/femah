using System.Web;
using Femah.Core.FeatureSwitchTypes;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Femah.Core.Tests
{
    public class PercentageFeatureSwitchTests
    {
        private HttpCookieCollection _cookies;
        private FemahContext _femahContext;

        [SetUp]
        public void Initialize()
        {
            // Initialise cookie collection.
            _cookies = new HttpCookieCollection();

            // Mock out the HttpContext - mock uses our local cookie collection.
            var httpContextMock = CreateContextMock();

            // Create new FemahContext using mock HttpContext.
            _femahContext = new FemahContext(httpContextMock.Object);
        }

        [Test]
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

        [Test]
        public void SetsCookieWhenNoCookieExists()
        {
            var featureSwitch = new PercentageFeatureSwitch
            {
                IsEnabled = true,
                Name = "testPercentageFeatureSwitch"
            };

            var result = featureSwitch.IsOn(_femahContext);

            _cookies.Count.ShouldBe(1);
            _cookies[0].Name.ShouldBe(featureSwitch.Name);
            _cookies[0].Value.ShouldBe(result.ToString());
        }

        [Test]
        public void IsOnIffRandomNumberBelowThreshold()
        {
            // Random number below threshold (should be on).
            var featureSwitch = CreateTestPercentageSwitch(0.1, 30);

            var result = featureSwitch.IsOn(_femahContext);
            result.ShouldBe(true);

            // Random number above threshold (should be off).
            featureSwitch = CreateTestPercentageSwitch(0.4, 30);

            _cookies.Clear();
            result = featureSwitch.IsOn(_femahContext);
            result.ShouldBe(false);

            // Random number same as threshold (should be off).
            featureSwitch = CreateTestPercentageSwitch(0.1, 10);

            _cookies.Clear();
            result = featureSwitch.IsOn(_femahContext);
            result.ShouldBe(false);


            // Random number same as threshold, threshold is zero. (Should be off).
            featureSwitch = CreateTestPercentageSwitch(0.0, 0);

            _cookies.Clear();
            result = featureSwitch.IsOn(_femahContext);
            result.ShouldBe(false);
        }

        private static PercentageFeatureSwitch CreateTestPercentageSwitch(double randomValue, int percentageOn)
        {
            return new PercentageFeatureSwitch(() => randomValue)
            {
                PercentageOn = percentageOn,
                Name = "testfeatureswitch",
                IsEnabled = true
            };
        }

        private Mock<HttpContextBase> CreateContextMock()
        {
            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(c => c.Request.Cookies).Returns(_cookies);
            httpContextMock.Setup(c => c.Response.Cookies).Returns(_cookies);
            return httpContextMock;
        }
    }
}
