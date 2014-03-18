using System.Web;

namespace Femah.Core.UI
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
