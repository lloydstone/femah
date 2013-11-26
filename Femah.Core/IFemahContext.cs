using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Femah.Core
{
    public interface IFemahContext
    {
        HttpContextBase HttpContext { get; }
    }
}
