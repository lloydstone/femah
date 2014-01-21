using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Femah.Core
{
    /// <summary>
    /// The context in which a FeatureSwitch decides if it's on or off.
    /// Custom-built feature switches may require app-specific information,
    /// which would be passed in using this context.
    /// </summary>
    public interface IFemahContext
    {
        HttpContextBase HttpContext { get; }
    }
}
