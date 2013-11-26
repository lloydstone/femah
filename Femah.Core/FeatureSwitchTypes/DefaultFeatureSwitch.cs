using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Web.UI;

namespace Femah.Core.FeatureSwitchTypes
{
    [DataContract]
    public class DefaultFeatureSwitch : IFeatureSwitch
    {
        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public string Name { get;set;}

        [DataMember]
        public string FeatureType { get; set; }

        public bool IsOn( IFemahContext context )
        {
            return true;
        }

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
