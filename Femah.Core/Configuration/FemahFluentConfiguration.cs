using System;
using System.Linq;
using System.Reflection;

namespace Femah.Core.Configuration
{
    public class FemahFluentConfiguration
    {
        protected FemahConfiguration _config = new FemahConfiguration();

        public FemahFluentConfiguration Provider(IFeatureSwitchProvider provider)
        {
            _config.Provider = provider;
            return this;
        }

        public FemahFluentConfiguration ContextFactory(IFemahContextFactory contextFactory)
        {
            _config.ContextFactory = contextFactory;
            return this;
        }

        /// <summary>
        /// Provides the ability to configure femah with additional FeatureSwitchTypes from a given assembly. Used when 
        /// extending femah with custom FeatureSwitchTypes residing in a non-femah assembly.
        /// </summary>
        /// <param name="assembly">The target assembly to configure custom FeatureSwitchTypes from</param>
        /// <returns type="FemahFluentConfiguration" />
        public FemahFluentConfiguration AdditionalFeatureSwitchTypesFromAssembly(Assembly assembly)
        {
            // Get all feature switch types from the assembly.
            var types = assembly.GetExportedTypes();
            foreach (var t in types)
            {
                if (t.GetInterfaces().Contains(typeof(IFeatureSwitch)))
                {
                    _config.CustomSwitchTypes.Add(t);
                }
            }
            return this;
        }

        /// <summary>
        /// Provides the ability to pass in and configure femah with a predefined collection of FeatureSwitchTypes, i.e. those 
        /// types implementing IFeatureSwitch.  Predominantly used for unit testing purposes, allowing us to configure femah with
        /// a known subset of FeatureSwitchTypes.
        /// </summary>
        /// <param name="types" type="Type[]">An array containing the FeatureSwitchTypes to configure</param>
        /// <returns type="FemahFluentConfiguration" />
        public FemahFluentConfiguration WithSelectedFeatureSwitchTypes(Type[] types)
        {
            foreach (var t in types.Where(t => t.GetInterfaces().Contains(typeof(IFeatureSwitch))))
            {
                _config.StandardSwitchTypes.Add(t);
            }
            return this;
        }

        public FemahFluentConfiguration FeatureSwitchEnum(Type type)
        {
            _config.FeatureSwitchEnumType = type;
            return this;
        }

        public void Initialise()
        {
            Femah.Initialise(_config);
        }
    }
}
