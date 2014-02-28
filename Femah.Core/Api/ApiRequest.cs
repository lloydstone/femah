using System.Net;

namespace Femah.Core.Api
{
    public class ApiRequest
    {
        public ApiService? Service { get; set; }
        public string Parameter { get; set; }
        public string HttpMethod { get; set; }
        public string Body { get; set; }
        public string ErrorMessage { get; set; }
        public HttpStatusCode ErrorMessageHttpStatusCode { get; set; }

        public enum ApiService
        {
            featureswitch,
            featureswitches,
            featureswitchtypes
        }
    }
}