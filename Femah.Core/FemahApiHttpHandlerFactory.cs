using System.Web;
using Femah.Core.Api;

namespace Femah.Core
{
    public class FemahApiHttpHandlerFactory : IHttpHandlerFactory
    {
        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            return new FemahApiHttpHandler();
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
        }
    }
}