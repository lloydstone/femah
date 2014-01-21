using System.Web;

namespace Femah.Core
{
    /// <summary>
    /// A default factory for generating FemahContexts.
    /// </summary>
    public class FemahContextFactory : IFemahContextFactory
    {
        /// <summary>
        /// Generate a FemahContext to be passed to a FeatureSwitch's IsOn method.
        /// </summary>
        /// <returns>A FemahContext object.</returns>
        public IFemahContext GenerateContext()
        {
            HttpContextWrapper contextWrapper = null;

            if (HttpContext.Current != null)
            {
                contextWrapper = new HttpContextWrapper(HttpContext.Current);
            }

            return new FemahContext(contextWrapper);
        }
    }
}
