using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.Web;
using System.Collections.Specialized;

namespace Femah.Core
{
    public class FeatureSwitching
    {
        private static IFeatureSwitchProvider _provider = null;
        private static Dictionary<int,string> _switches = null;
        private static List<Type> _switchTypes = null;

        private static IFemahContextFactory _contextFactory { get; set; }

        /// <summary>
        /// Initialise the feature switching engine.
        /// </summary>
        /// <param name="provider">The feature switch provider to use to persist feature switches.</param>
        public static void Initialise(IFeatureSwitchProvider provider = null, IFemahContextFactory contextFactory = null)
        {
            // Save provider.
            _provider = provider ?? new DefaultFeatureSwitchProvider();

            // Scan assemblies for feature switches.
            _switches = FeatureSwitching.LoadFeatureSwitchesFromAssembly(Assembly.GetCallingAssembly());

            // Scan assemblies for feature switch types.
            _switchTypes = FeatureSwitching.LoadFeatureSwitchTypesFromAssembly(Assembly.GetExecutingAssembly());
            _switchTypes.AddRange(FeatureSwitching.LoadFeatureSwitchTypesFromAssembly(Assembly.GetCallingAssembly()));

            // Inialise the provider.
            _provider.Initialise(_switches.Values.ToList());

            if (contextFactory != null)
                _contextFactory = contextFactory;
            else
                _contextFactory = new FemahContextFactory();

            return;
        }

        /// <summary>
        /// Is a feature switch turned on?
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsFeatureOn( int id )
        {
            if (!_switches.ContainsKey(id))
                return false;

            // Load feature switch.
            var name = _switches[id];
            var featureSwitch = _provider.Get(name);

            // Generate context.
           // var context = new FemahContext(new HttpContextWrapper(HttpContext.Current));
            var context = _contextFactory.GenerateContext();

            return featureSwitch.IsEnabled && featureSwitch.IsOn(context);
        }

        /// <summary>
        /// Enable/disable a feature switch.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="enabled"></param>
        public static void SetFeature(string name, bool enabled)
        {
            // Load feature switch.
            var featureSwitch = _provider.Get(name);
            if (featureSwitch != null)
            {
                featureSwitch.IsEnabled = enabled;
                _provider.Save(featureSwitch);
            }
        }

        public static void SetSwitchType(string name, string typeName)
        {
            // Load feature switch.
            var featureSwitch = _provider.Get(name);

            // Get type based on assembly qualified name.
            var type = _switchTypes.FirstOrDefault( t => String.Equals(t.AssemblyQualifiedName, typeName) );

            if ( featureSwitch == null || type == null )
            {
                return;
            }
            
            // Create instance of new type and copy standard IFeatureSwitch values across.
            var newFeatureSwitch = Activator.CreateInstance(type) as IFeatureSwitch;
            if (newFeatureSwitch != null)
            {
                newFeatureSwitch.Name = featureSwitch.Name;
                newFeatureSwitch.IsEnabled = featureSwitch.IsEnabled;
                newFeatureSwitch.FeatureType = featureSwitch.GetType().Name;
            }

            // Save as the new type of feature switch.
            _provider.Save(newFeatureSwitch);
        }

        public static List<IFeatureSwitch> AllFeatures()
        {
            return _provider.All();
        }

        public static List<Type> AllSwitchTypes()
        {
            return _switchTypes;
        
        }

        public static IFeatureSwitch GetFeature(string featureName)
        {
            return _provider.Get(featureName);
        }

        public static void SetFeatureAttributes(string featureName, NameValueCollection values)
        {
            var feature = _provider.Get(featureName);
            if (feature != null)
            {
                feature.SetCustomAttributes(values);
                _provider.Save(feature);
            }
        }

        #region Private Methods

        private static Dictionary<int,string> LoadFeatureSwitchesFromAssembly(Assembly assembly)
        {
            var featureList = new Dictionary<int,string>();

            // Build a list of feature switch names.
            var types = assembly.GetExportedTypes();
            var featureSwitches = types.FirstOrDefault(t => String.Equals(t.Name, "FemahFeatureSwitches", StringComparison.InvariantCulture));
            if (featureSwitches != null)
            {
                var names = Enum.GetNames(featureSwitches);
                var values = Enum.GetValues(featureSwitches);

                for (int i = 0; i < names.Count(); i++)
                {
                    featureList.Add((int)(values.GetValue(i)), names[i]);
                }
            }

            return featureList;
        }

        private static List<Type> LoadFeatureSwitchTypesFromAssembly(Assembly assembly)
        {
            var typeList = new List<Type>();

            // Get all feature switch types from the Femah assembly.
            var types = assembly.GetExportedTypes();
            foreach (var t in types)
            {
                if (t.GetInterfaces().Contains(typeof(IFeatureSwitch)))
                {
                    typeList.Add(t);
                }
            }

            return typeList;
        }

        #endregion
    }
}
