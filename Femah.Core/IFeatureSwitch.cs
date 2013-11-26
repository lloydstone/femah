using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace Femah.Core
{

    public interface IFeatureSwitch
    {
        /// <summary>
        /// Overall enabler/disabler of feature switch.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Name of feature switch.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Return true if the feature is on.
        /// </summary>
        /// <returns></returns>
        bool IsOn( IFemahContext context );

        string FeatureType { get; set; }

        /// <summary>
        /// The feature switch should render any custom UI for the admin screen.
        /// </summary>
        /// <param name="writer"></param>
        void RenderUI(HtmlTextWriter writer);

        /// <summary>
        /// Set any custom attributes the feature switch type requires.
        /// </summary>
        /// <param name="values">A collection of key-value pairs.</param>
        void SetCustomAttributes(NameValueCollection values);
    }
}
