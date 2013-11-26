using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Femah.Core
{
    public class FemahContext : IFemahContext
    {
        public HttpContextBase HttpContext { get; private set; }

        public FemahContext(HttpContextBase httpContext)
        {
            this.HttpContext = httpContext;
        }
    }
}
