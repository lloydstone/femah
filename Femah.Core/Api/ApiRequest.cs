namespace Femah.Core.Api
{
    public class ApiRequest
    {
        public ApiService? Service { get; set; }
        public string Parameter { get; set; }
        public string HttpMethod { get; set; }
        public string ErrorMessage { get; set; }

        public enum ApiService
        {
            featureswitch,
            featureswitches,
            featureswitchtype,
            featureswitchtypes
        }
    }
}