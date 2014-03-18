using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Femah.Core
{

    public interface IFeatureSwitch : IEquatable<IFeatureSwitch>
    {
        /// <summary>
        /// Overall enabler/disabler of feature switch, is the feature switch enabled for use?
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// The name of the feature switch.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Given a Femah context, determine if the feature should be on or off.  
        /// </summary>
        /// <returns name="IsOn" type="boolean">Returns true if the feature is on.</returns>
        bool IsOn( IFemahContext context );

        /// <summary>
        /// The fully qualified assembly name of the feature switch type, used when serializing/deserialising to/from JSON.
        /// </summary>
        string FeatureType { get; set; }

        /// <summary>
        /// Render the UI required by the feature switch in the admin pages.
        /// Feature switches that require custom parameters should override this.
        /// </summary>
        /// <param name="writer" type="HtmlTextWriter">The HtmlTextWriter to be used to render the HTML.</param>
        void RenderUi(HtmlTextWriter writer);

        /// <summary>
        /// Given a dictionary of strings, set any custom attributes the feature switch requires.
        /// </summary>
        /// <param name="values" type="Dictionary">A dictionary of strings.</param>
        void SetCustomAttributes(Dictionary<string, string> values);

        /// <summary>
        /// Retrieve any custom attributes the feature switch may have set.
        /// </summary>
        /// <returns type="Dictionary">A dictionary of strings representing the feature switch's custom attributes.</returns>
        Dictionary<string, string> GetCustomAttributes();

        /// <summary>
        /// A short description of the feature switch type and how it works. 
        /// </summary>
        string Description { get; }

        /// <summary>
        /// A more detailed description of how the feature switch type is configured for use.
        /// </summary>
        string ConfigurationInstructions { get; }

        /// <summary>
        /// Provides the ability to compare two <type>IFeatureSwitch</type> instances to determine their equality.
        /// </summary>
        /// <param name="other" type="IFeatureSwitch">The <type>IFeatureSwitch</type> instance to compare to this instance</param>
        /// <returns type="bool">A boolean signifying whether the two <type>IFeatureSwitch</type> instances are equal</returns>
        new bool Equals(IFeatureSwitch other);
    }
}
