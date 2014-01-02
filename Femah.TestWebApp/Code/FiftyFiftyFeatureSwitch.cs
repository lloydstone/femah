using System;
using System.Web.UI;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using Femah.Core;
using Femah.Core.FeatureSwitchTypes;

namespace Femah.TestWebApp.Code
{
    /// <summary>
    /// A feature switch that only enables the feature half the time.
    /// </summary>
    [DataContract]
    public class FiftyFiftyFeatureSwitch : FeatureSwitchBase
    {
        public override bool IsOn( IFemahContext context )
        {
            var customContext = context as CustomFemahContext;
            if ( customContext == null ) 
                return false;

            Random r = new Random();
            return true;
        }
    }
}