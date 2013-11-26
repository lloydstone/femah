using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Femah.Core;

namespace Femah.Ui
{
    public class FemahHttpHandlerFactory : IHttpHandlerFactory
    {
        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            return new FemahHttpHandler();
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
        }
    }
}
