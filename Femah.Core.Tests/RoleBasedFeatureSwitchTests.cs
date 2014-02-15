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
        private FemahContext _femahContext;
        private const string _testUserRole = "testrole";

        [SetUp]
        public void Initialize()
        {
            // Mock out the HttpContext - mock uses our local cookie collection.
            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(c => c.User.Identity.IsAuthenticated)
                .Returns(true);
            httpContextMock.Setup(c => c.User.IsInRole(It.IsAny<string>()))
                .Returns((string s) => String.Equals(s, _testUserRole, StringComparison.InvariantCultureIgnoreCase));

            // Create new FemahContext using mock HttpContext.
            _femahContext = new FemahContext(httpContextMock.Object);
        }

        [Test]
        public void ReturnsTrueWhenUserInRole()
        {
            var featureSwitch = new RoleBasedFeatureSwitch
            {
                IsEnabled = true,
                Name = "testRoleBasedFeatureSwitch"
            };
            featureSwitch.AcceptedRoles.Add(_testUserRole);

            var result = featureSwitch.IsOn(_femahContext);
            result.ShouldBe(true);
        }

        [Test]
        public void ReturnsFalseWhenUserNotInRole()
        {
            var featureSwitch = new RoleBasedFeatureSwitch
            {
                IsEnabled = true,
                Name = "testRoleBasedFeatureSwitch"
            };
            featureSwitch.AcceptedRoles.Add("adifferentrole");

            var result = featureSwitch.IsOn(_femahContext);
            result.ShouldBe(false);
        }

        [Test]
        public void ReturnsFalseWhenNoRoles()
        {
            var featureSwitch = new RoleBasedFeatureSwitch
            {
                IsEnabled = true,
                Name = "testRoleBasedFeatureSwitch"
            };

            var result = featureSwitch.IsOn(_femahContext);
            result.ShouldBe(false);
        }
        
        [Test]
        public void ReturnsTrueWhenUserInOneOfManyRoles()
        {
            var featureSwitch = new RoleBasedFeatureSwitch
            {
                IsEnabled = true,
                Name = "testRoleBasedFeatureSwitch"
            };

            featureSwitch.AcceptedRoles.AddRange( new [] { "adifferentrole", "role2", _testUserRole });

            var result = featureSwitch.IsOn(_femahContext);
            result.ShouldBe(true);
        }
    }
}
