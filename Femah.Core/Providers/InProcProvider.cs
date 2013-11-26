using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Femah.Core.FeatureSwitchTypes;

namespace Femah.Core
{
    /// <summary>
    /// A feature switch provider that stores switches in memory on the web server.
    /// </summary>
    public class InProcProvider : IFeatureSwitchProvider
    {
        static List<IFeatureSwitch> _featureSwitches = new List<IFeatureSwitch>();

        /// <summary>
        /// Initialise the feature switches in the provider.
        /// </summary>
        /// <param name="featureSwitches"></param>
        public void Initialise( IEnumerable<string> featureSwitches )
        {
            _featureSwitches.Clear();

            foreach (var featureSwitch in featureSwitches)
            {
                _featureSwitches.Add(new SimpleFeatureSwitch { Name = featureSwitch, IsEnabled = false, FeatureType = featureSwitch.GetType().Name});
            }

            return;
        }

        /// <summary>
        /// Load the feature switch (and type).
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IFeatureSwitch Get(string name)
        {
            return _featureSwitches.FirstOrDefault(fs => String.Equals(fs.Name, name, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Save the feature switch's details and type.
        /// </summary>
        /// <param name="featureSwitch"></param>
        public void Save(IFeatureSwitch featureSwitch)
        {
            var currentFeatureSwitch = _featureSwitches.FirstOrDefault(fs => String.Equals(fs.Name, featureSwitch.Name, StringComparison.InvariantCultureIgnoreCase));

            if (currentFeatureSwitch != null)
            {
                _featureSwitches.Remove(currentFeatureSwitch);
                _featureSwitches.Add(featureSwitch);
            }
        }

        /// <summary>
        /// Return all feature switches in the provider.
        /// </summary>
        /// <returns></returns>
        public List<IFeatureSwitch> All()
        {
            return _featureSwitches;
        }
    }
}
