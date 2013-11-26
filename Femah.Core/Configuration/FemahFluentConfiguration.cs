using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Femah.Core
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

        public FemahFluentConfiguration AdditionalSwitchTypesFromAssembly(Assembly assembly)
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

        public FemahFluentConfiguration FeatureSwitchEnum(Type type)
        {
            _config.FeatureSwitchEnumType = type;
            return this;
        }

        public void Initialise()
        {
            FeatureSwitching.Initialise(_config);
        }
    }
}
