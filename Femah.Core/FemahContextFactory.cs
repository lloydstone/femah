using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Femah.Core
{
    public class FemahContextFactory : IFemahContextFactory
    {
        public IFemahContext GenerateContext()
        {
            HttpContextWrapper contextWrapper = null;

            if (HttpContext.Current != null)
            {
                contextWrapper = new HttpContextWrapper(HttpContext.Current);
            }

            return new FemahContext(contextWrapper);
        }
    }
}
