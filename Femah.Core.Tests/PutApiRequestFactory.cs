using Femah.Core.Api;

namespace Femah.Core.Tests
{
    public class PutApiRequestFactory
    {
        private string _body = string.Empty;
        private string _parameterName = string.Empty;
        private ApiRequest.ApiService _service = ApiRequest.ApiService.featureswitches;

        public PutApiRequestFactory ForServiceType(ApiRequest.ApiService service)
        {
            _service = service;
            return this;
        }

        public PutApiRequestFactory WithParameterName(string name)
        {
            _parameterName = name;
            return this;
        }

        public PutApiRequestFactory WithBody(string body)
        {
            _body = body;
            return this;
        }

        public ApiRequest Build()
        {
            return new ApiRequest
            {
                HttpMethod = "PUT",
                Service = _service,
                Parameter = _parameterName,
                Body = _body
            };
        }
    }
}