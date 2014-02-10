using System;
using System.Collections.Generic;

namespace Femah.Core
{
    /// <summary>
    /// Persists/loads feature switches.
    /// </summary>
    public interface IFeatureSwitchProvider
    {
        /// <summary>
        /// Initialise the provider, given the names of the feature switches.
        /// </summary>
        /// <param name="featureSwitches">Names of the feature switches in the application.</param>
        /// <param name="featureSwitchTypes">The feature switch types in the application.</param>
        void Initialise(IEnumerable<string> featureSwitches, List<Type> featureSwitchTypes );

        /// <summary>
        /// Get a feature switch.
        /// </summary>
        /// <param name="name">The name of the feature switch to get</param>
        /// <returns>An instance of IFeatureSwitch if found, otherwise null</returns>
        IFeatureSwitch Get(string name);

        /// <summary>
        /// Save a feature switch.
        /// </summary>
        /// <param name="featureSwitch">The feature to be saved</param>
        void Save(IFeatureSwitch featureSwitch);

        /// <summary>
        /// Return all feature switches.
        /// </summary>
        /// <returns>A list of zero or more instances of IFeatureSwitch</returns>
        List<IFeatureSwitch> AllFeatureSwitches();

        List<Type> AllFeatureSwitchTypes();
    }
}
