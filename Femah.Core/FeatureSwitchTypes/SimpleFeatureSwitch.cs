using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Web.UI;

namespace Femah.Core.FeatureSwitchTypes
{
    public class SimpleFeatureSwitch : FeatureSwitchBase
    {
        public override bool IsOn( IFemahContext context )
        {
            // The simple feature switch is always on.
            return true;
        }
    }
}
