using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

namespace Femah.Core.FeatureSwitchTypes
{
    /// <summary>
    /// A generic base class, implementing default behaviour for the IFeatureSwitch interface.
    /// </summary>
    public abstract class FeatureSwitchBase : IFeatureSwitch, IEquatable<IFeatureSwitch>
    {
        /// <summary>
        /// Overall enabler/disabler of feature switch, is the feature switch enabled for use?
        /// </summary>
        public virtual bool IsEnabled { get; set; }

        /// <summary>
        /// The name of the feature switch.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Given a Femah context, determine if the feature should be on or off.  
        /// </summary>
        /// <returns name="IsOn" type="boolean">Returns true if the feature is on.</returns>
        public abstract bool IsOn(IFemahContext context);

        /// <summary>
        /// The fully qualified assembly name of the feature switch type, used when serializing/deserialising to/from JSON.
        /// </summary>
        public virtual string FeatureType { get; set; }

        /// <summary>
        /// Render the UI required by the feature switch in the admin pages.
        /// Feature switches that require custom parameters should override this.
        /// </summary>
        /// <param name="writer" type="HtmlTextWriter">The HtmlTextWriter to be used to render the HTML.</param>
        public virtual void RenderUi(HtmlTextWriter writer)
        {
            return;
        }

        /// <summary>
        /// Given a dictionary of strings, set any custom attributes the feature switch requires.
        /// </summary>
        /// <param name="values" type="Dictionary">A dictionary of strings.</param>
        public virtual void SetCustomAttributes(Dictionary<string, string> values)
        {
            return;
        }

        /// <summary>
        /// Given a dictionary of strings, set any custom attributes the feature switch requires.
        /// </summary>
        /// <param name="values" type="Dictionary">A dictionary of strings.</param>
        public virtual Dictionary<string, string> GetCustomAttributes()
        {
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// A short description of the feature switch type and how it works. 
        /// </summary>
        public virtual string Description
        {

            get { return "Define a short description of the feature switch type here."; }
        }

        /// <summary>
        /// A more detailed description of how the feature switch type is configured for use.
        /// </summary>
        public virtual string ConfigurationInstructions
        {
            get { return "Add configuration context and instructions to be displayed in the admin UI"; }
        }

        /// <summary>
        /// Provides the ability to compare two <type>IFeatureSwitch</type> instances to determine their equality.
        /// </summary>
        /// <param name="other" type="IFeatureSwitch">The <type>IFeatureSwitch</type> instance to compare to this instance</param>
        /// <returns type="bool">A boolean signifying whether the two <type>IFeatureSwitch</type> instances are equal</returns>
        public bool Equals(IFeatureSwitch other)
        {
            if (other == null)
                return false;

            //Primarily for testing, to force us to set a name in mock FeatureSwitch objects. This should never happen in reality.
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(other.Name))
                return false;

            if (Name != other.Name)
                return false;

            if (FeatureType != other.FeatureType)
                return false;

            if (IsEnabled != other.IsEnabled)
                return false;

            if (!GetCustomAttributes().SequenceEqual(other.GetCustomAttributes()))
                return false;

            return true;
        }
    }
}
