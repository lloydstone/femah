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
        [TestCase(0.1, 30, true)] // Random number below threshold (should be on).
        [TestCase(0.4, 30, false)] // Random number above threshold (should be off).
        [TestCase(0.1, 10, false)] // Random number same as threshold (should be off).
        [TestCase(0.0, 0, false)] // Random number same as threshold, threshold is zero. (Should be off).
        public void IsOnIffRandomNumberBelowThreshold(double randomNumber, int percentageOn, bool expectedResult)
        {
            var featureSwitch = CreateTestPercentageSwitch(randomNumber, percentageOn);

            var result = featureSwitch.IsOn(_femahContext);
            result.ShouldBe(expectedResult);
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
