namespace Femah.Core.Api
{
    public class ApiRequest
    {
        public ApiCollection Collection { get; set; }
        public string Member { get; set; }
        public string HttpMethod { get; set; }
        public string ErrorMessage { get; set; }

        public enum ApiCollection
        {
            FeatureSwitch,
            FeatureSwitches,
            FeatureSwitchType,
            FeatureSwitchTypes
        }
    }
}