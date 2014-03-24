namespace Femah.Core.Tests
{
    using NUnit.Framework;

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
    }
}