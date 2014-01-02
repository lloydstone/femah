﻿using System;
using System.Runtime.Remoting.Contexts;
using System.Web;
using Femah.Core.FeatureSwitchTypes;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Femah.Core.Tests
{
    public class FeatureSwitchingTests
    {
        public enum FeatureSwitches
        {
            SomeNewFeature = 1
        }

        #region Exception Handling Tests

        [Test]
        public void InvokingWithoutInitialisingDoesntThrowException()
        {
            FeatureSwitching.IsFeatureOn((int)FeatureSwitches.SomeNewFeature);
        }

        [Test]
        public void ExceptionsThrownByProviderAreSwallowed()
        {
            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get(It.IsAny<string>()))
                .Throws(new Exception("Exception thrown by provider."));

            FeatureSwitching.Configure()
                .Provider(providerMock.Object)
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Initialise();

            FeatureSwitching.IsFeatureOn((int)FeatureSwitches.SomeNewFeature);            
        }

        [Test]
        public void ExceptionsThrownByContextFactoryAreSwallowed()
        {

            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get(It.IsAny<string>()))
                .Returns(new SimpleFeatureSwitch { IsEnabled = true });

            var contextFactoryMock = new Mock<IFemahContextFactory>();
            contextFactoryMock.Setup(f => f.GenerateContext())
                .Throws(new Exception("Exception thrown by context factory."));

            FeatureSwitching.Configure()
                .Provider(providerMock.Object)
                .ContextFactory(contextFactoryMock.Object)
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Initialise();

            FeatureSwitching.IsFeatureOn((int)FeatureSwitches.SomeNewFeature);
        }

        [Test]
        public void ExceptionsThrownByContextAreSwallowed()
        {
            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get(It.IsAny<string>()))
                .Returns( new SimpleFeatureSwitch { IsEnabled = true });

            var contextMock = new Mock<IFemahContext>();
            contextMock.SetupGet(c => c.HttpContext)
                .Throws(new Exception("Exception thrown by context."));

            var contextFactoryMock = new Mock<IFemahContextFactory>();
            contextFactoryMock.Setup(f => f.GenerateContext())
                .Returns(contextMock.Object);

            FeatureSwitching.Configure()
                .Provider(providerMock.Object)
                .ContextFactory(contextFactoryMock.Object)
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Initialise();

            FeatureSwitching.IsFeatureOn((int)FeatureSwitches.SomeNewFeature);
        }

        [Test]
        public void ExceptionsThrownByFeatureSwitchesAreSwallowed()
        {
            var featureSwitchMock = new Mock<IFeatureSwitch>();
            featureSwitchMock.Setup(fs => fs.IsOn(It.IsAny<IFemahContext>()))
                .Throws(new Exception("Exception thrown by feature switch."));
            featureSwitchMock.SetupGet(fs => fs.IsEnabled).Returns(true);

            var providerMock = new Mock<IFeatureSwitchProvider>();
            providerMock.Setup(p => p.Get(It.IsAny<string>()))
                .Returns(featureSwitchMock.Object);

            FeatureSwitching.Configure()
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Provider(providerMock.Object)
                .Initialise();

            FeatureSwitching.IsFeatureOn((int)FeatureSwitches.SomeNewFeature);
        }

        [Test]
        public void InvokingWithInvalidFeatureSwitchIdDoesntThrowException()
        {
            FeatureSwitching.Configure()
                .FeatureSwitchEnum(typeof(FeatureSwitches))
                .Initialise();

            var result = FeatureSwitching.IsFeatureOn(100);

            result.ShouldBe(false);
        }

        #endregion
    }
}
