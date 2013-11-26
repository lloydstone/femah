using System;
using System.Web.UI;
using Femah.Core;
using System.Runtime.Serialization;
using System.Collections.Specialized;

namespace Femah.TestWebApp.Code
{
    /// <summary>
    /// A feature switch that only enables the feature half the time.
    /// </summary>
    [DataContract]
    public class FiftyFiftyFeatureSwitch : IFeatureSwitch
    {
        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public string Name { get; set; }


        public bool IsOn( IFemahContext context )
        {
            var customContext = context as CustomFemahContext;
            if ( customContext == null ) 
                return false;

            Random r = new Random();
            return true;
        }

        [DataMember]
        public string FeatureType { get; set; }

        public void RenderUI(HtmlTextWriter writer)
        {
            return;
        }

        public void SetCustomAttributes(NameValueCollection values)
        {
            return;
        }
    }
}