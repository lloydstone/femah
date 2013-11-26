using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Femah.Core;

namespace Femah.TestWebApp.Models
{
    public class FemahUIModel
    {
        public List<Type> SwitchTypes { get; set; }
        public List<IFeatureSwitch> FeatureSwitches { get; set; }
    }
}