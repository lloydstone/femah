using System;

namespace Femah.Core.Api
{
    public class ApiFeatureSwitchType
    {
        public string Name { get; set; }
        public Type FeatureSwitchType { get; set; }
        public string Description { get; set; }
        public string ConfigurationInstructions { get; set; }
    }
}