using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Femah.Core.FeatureSwitchTypes
{
    /// <summary>
    /// A simple feature switch that is on for a set percentage of users.
    /// The state of the switch is persisted in the user's cookies.
    /// If no cookie exists the state is chosen at random (weighted according to the percentage), 
    /// and then stored in a cookie.
    /// </summary>
    public class PercentageFeatureSwitch : FeatureSwitchBase
    {
        private Random _random;
        private Func<double> _randomGenerator = null;

        /// <summary>
        /// The percentage of users who should see this feature.
        /// Eg. 10 means the feature is on for 10% of users.
        /// </summary>
        public int PercentageOn { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PercentageFeatureSwitch()
        {
            _random = new Random();
            _randomGenerator = _random.NextDouble;
        }

        /// <summary>
        /// Initialise a PercentageFeatureSwitch with a custom random-number generator function.
        /// </summary>
        /// <param name="randomGenerator">A function which returns a random value between 0.0 and 1.0 on each call.</param>
        public PercentageFeatureSwitch(Func<double> randomGenerator)
        {
            _randomGenerator = randomGenerator;
        }

        #region IFeatureSwitch members

        public override bool IsOn(IFemahContext context)
        {
            bool isOn;

            if ( context.HttpContext == null || context.HttpContext.Request == null )
            {
                return false;
            }

            // Check if a cookie for this switch exists already.
            if ( context.HttpContext.Request.Cookies.AllKeys.Contains(this.Name) )
            {
                if (!Boolean.TryParse(context.HttpContext.Request.Cookies[this.Name].Value, out isOn))
                {
                    isOn = false;
                }
            }
            else
            {
                // No cookie set.  Choose randomly if feature should be set or not.
                double threshold = PercentageOn / 100.0;
                isOn = _randomGenerator() < threshold;

                // Save value to cookie.
                context.HttpContext.Response.Cookies.Add( new HttpCookie(this.Name, isOn.ToString()) );
            }

            return isOn;
        }

        public override void RenderUI(HtmlTextWriter writer)
        {
            writer.Write("Percentage: <input type='text' name='percentage' value='{0}'/>", this.PercentageOn);
            return;
        }

        public override void SetCustomAttributes(NameValueCollection values)
        {
            string percentage = values["percentage"];
            int percentageValue;
            if (!String.IsNullOrEmpty(percentage) && int.TryParse(percentage, out percentageValue) )
            {
                this.PercentageOn = percentageValue;
            }
        }

        #endregion
        public override string Description
        {

            get { return "A simple feature switch that is on for a set percentage of users. The state of the switch is persisted in the user's cookies.If no cookie exists the state is chosen at random (weighted according to the percentage), and then stored in a cookie."; }
        }

        public override string ConfigurationInstructions
        {
            get { return "Set PercentageOn to the percentage of users who should see this feature. Eg. 10 means the feature is on for 10% of users."; }
        }

    }
}
