using System;
using Moq;
using NUnit.Framework;
using Femah.Core.FeatureSwitchTypes;

namespace Femah.Core.Tests
{
    public class WhenCallingIsFeatureOn
    {
        public enum FeatureSwitches
        {
            SomeNewFeature = 1
        }

        [Test]
        public void AndInvokingWithoutInitialising_ThenNoExceptionIsThrown()
        {
            Femah.IsFeatureOn((int)FeatureSwitches.SomeNewFeature);
        }

        [Test]
        public void AndExceptionIsThrownByProvider_ThenTheExceptionIsSwallowed()
        {
            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get(It.IsAny<string>()))
                .Throws(new Exception("Exception thrown by provider."));

            Femah.Configure()
                .Provider(providerMock.Object)
                .FeatureSwitchEnum(typeof(FemahTests.FeatureSwitches))
                .Initialise();

            Femah.IsFeatureOn((int)FemahTests.FeatureSwitches.SomeNewFeature);
        }

        [Test]
        public void AndExceptionIsThrownByContextFactory_ThenTheExceptionIsSwallowed()
        {
            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get(It.IsAny<string>()))
                .Returns(new SimpleFeatureSwitch { IsEnabled = true });

            var contextFactoryMock = new Mock<IFemahContextFactory>();
            contextFactoryMock.Setup(f => f.GenerateContext())
                .Throws(new Exception("Exception thrown by context factory."));

            Femah.Configure()
                .Provider(providerMock.Object)
                .ContextFactory(contextFactoryMock.Object)
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Initialise();

            Femah.IsFeatureOn((int)FeatureSwitches.SomeNewFeature);
        }

        [Test]
        public void AndExceptionIsThrownByContext_ThenTheExceptionIsSwallowed()
        {
            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get(It.IsAny<string>()))
                .Returns(new SimpleFeatureSwitch { IsEnabled = true });

            var contextMock = new Mock<IFemahContext>();
            contextMock.SetupGet(c => c.HttpContext)
                .Throws(new Exception("Exception thrown by context."));

            var contextFactoryMock = new Mock<IFemahContextFactory>();
            contextFactoryMock.Setup(f => f.GenerateContext())
                .Returns(contextMock.Object);

            Femah.Configure()
                .Provider(providerMock.Object)
                .ContextFactory(contextFactoryMock.Object)
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Initialise();

            Femah.IsFeatureOn((int)FeatureSwitches.SomeNewFeature);
        }
    }
}