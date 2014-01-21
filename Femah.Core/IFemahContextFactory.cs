
namespace Femah.Core
{
    /// <summary>
    /// A factory to generate a FemahContext to pass into a FeatureSwitch's IsOn method.
    /// </summary>
    public interface IFemahContextFactory
    {
        IFemahContext GenerateContext();
    }
}
