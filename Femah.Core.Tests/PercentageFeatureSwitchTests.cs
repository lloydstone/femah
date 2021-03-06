﻿using System.Web;
using Femah.Core.FeatureSwitchTypes;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Femah.Core.Tests
{
    public class PercentageFeatureSwitchTests
    {
        public class TheIsOnMethod
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
            [TestCase(true, Result = true, TestName="ReturnsTrueSwitchState_IfCookieExistsWithTrueValue")]
            [TestCase(false, Result = false, TestName="ReturnsFalseSwitchState_IfCookieExistsWithFalseValue")]
            public bool ReturnsSwitchState_DependantOnCookieValue(bool cookieValue)
            {
                var featureSwitch = new PercentageFeatureSwitch
                {
                    IsEnabled = true,
                    Name = "testPercentageFeatureSwitch"
                };

                _cookies.Add(new HttpCookie(featureSwitch.Name, cookieValue.ToString()));
                return featureSwitch.IsOn(_femahContext);
            }

            [Test]
            public void SetsCookie_IfNoCookieExists()
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
            [TestCase(0.1, 30, Result = true, TestName = "ReturnsTrueSwitchState_IfRandomNumberIsBelowThreshold")]
            [TestCase(0.4, 30, Result = false, TestName = "ReturnsFalseSwitchState_IfRandomNumberIsAboveThreshold")]
            [TestCase(0.1, 10, Result = false, TestName = "ReturnsFalseSwitchState_IfRandomNumberIsSameAsThreshold")]
            [TestCase(0.0, 0, Result = false, TestName = "ReturnsFalseSwitchState_IfRandomNumberIsSameAsThreshold_AndThresholdIsZero")]
            public bool ReturnsSwitchState_DependantOnComparisonOfRandomNumberToThreshold(double randomNumber, int percentageOn)
            {
                var featureSwitch = CreateTestPercentageSwitch(randomNumber, percentageOn);
                return featureSwitch.IsOn(_femahContext);
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
}
