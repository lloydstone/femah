using System;
using System.Web;
using Femah.Core.FeatureSwitchTypes;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Femah.Core.Tests
{
    public class RoleBasedFeatureSwitchTests
    {
        public class TheIsOnMethod
        {
            private FemahContext _femahContext;
            private const string _testUserRole = "testrole";
            private RoleBasedFeatureSwitch _featureSwitch;

            [SetUp]
            public void Initialize()
            {
                CreateTestFeatureSwitch();

                // Mock out the HttpContext - mock uses our local cookie collection.
                var httpContextMock = CreateHttpContextMock();

                // Create new FemahContext using mock HttpContext.
                _femahContext = new FemahContext(httpContextMock.Object);
            }

            [Test]
            public void ReturnsTrue_IfUserIsInRole()
            {
                _featureSwitch.AcceptedRoles.Add(_testUserRole);

                var result = _featureSwitch.IsOn(_femahContext);
                result.ShouldBe(true);
            }

            [Test]
            public void ReturnsFalse_IfUserIsNotInRole()
            {
                _featureSwitch.AcceptedRoles.Add("adifferentrole");

                var result = _featureSwitch.IsOn(_femahContext);
                result.ShouldBe(false);
            }

            [Test]
            public void ReturnsFalse_IfThereAreNoRoles()
            {
                var result = _featureSwitch.IsOn(_femahContext);
                result.ShouldBe(false);
            }

            [Test]
            public void ReturnsTrue_IfUserIsInOneOfManyRoles()
            {
                _featureSwitch.AcceptedRoles.AddRange(new[] { "adifferentrole", "role2", _testUserRole });

                var result = _featureSwitch.IsOn(_femahContext);
                result.ShouldBe(true);
            }

            private static Mock<HttpContextBase> CreateHttpContextMock()
            {
                var httpContextMock = new Mock<HttpContextBase>();
                httpContextMock.Setup(c => c.User.Identity.IsAuthenticated)
                    .Returns(true);
                httpContextMock.Setup(c => c.User.IsInRole(It.IsAny<string>()))
                    .Returns((string s) => String.Equals(s, _testUserRole, StringComparison.InvariantCultureIgnoreCase));
                return httpContextMock;
            }

            private void CreateTestFeatureSwitch()
            {
                _featureSwitch = new RoleBasedFeatureSwitch
                {
                    IsEnabled = true,
                    Name = "testRoleBasedFeatureSwitch"
                };
            }
        }
    }
}