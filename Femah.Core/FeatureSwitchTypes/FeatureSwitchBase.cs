using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Femah.Core.FeatureSwitchTypes
{
    /// <summary>
    /// A generic base class, implementing default behaviour for the IFeatureSwitch interface.
    /// </summary>
    public abstract class FeatureSwitchBase : IFeatureSwitch
    {
        /// <summary>
        /// Is the feature switch enabled.
        /// </summary>
        public virtual bool IsEnabled { get; set; }

        /// <summary>
        /// The name of the feature switch.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// TODO: .
        /// </summary>
        public virtual string FeatureType { get; set; }

        /// <summary>
        /// Given a Femah context, determine if the feature should be on or off.
        /// </summary>
        public abstract bool IsOn(IFemahContext context);

        /// <summary>
        /// Render the UI required by the feature switch in the admin pages.
        /// Feature switches that require custom parameters should override this.
        /// </summary>
        /// <param name="writer">The HtmlTextWriter to be used to render the HTML.</param>
        public virtual void RenderUI(System.Web.UI.HtmlTextWriter writer)
        {
            return;
        }

        /// <summary>
        /// Given a series of name value pairs, set the feature switch's custom attributes.
        /// </summary>
        /// <param name="values">A collection of name-value pairs.</param>
        public virtual void SetCustomAttributes(System.Collections.Specialized.NameValueCollection values)
        {
            return;
        }

        public virtual string Description
        {

            get { return "Define a short description of the feature switch type here."; }
        }

        public virtual string ConfigurationInstructions
        {
            get { return "Add configuration context and instructions to be displayed in the admin UI"; }
        }
    }
}
