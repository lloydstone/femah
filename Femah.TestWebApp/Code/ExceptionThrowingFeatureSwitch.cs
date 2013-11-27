using Femah.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Femah.TestWebApp.Code
{
    /// <summary>
    /// A test feature switch that always throw an exception.
    /// </summary>
    public class ExceptionThrowingFeatureSwitch : FeatureSwitchBase
    {
        public override bool IsOn(IFemahContext context)
        {
            throw new NotImplementedException();
        }
    }
}