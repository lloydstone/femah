using System.Web;

namespace Femah.Core
{
    /// <summary>
    /// Default FemahContext.
    /// </summary>
    public class FemahContext : IFemahContext
    {
        public HttpContextBase HttpContext { get; private set; }

        public FemahContext(HttpContextBase httpContext)
        {
            this.HttpContext = httpContext;
        }
    }
}
