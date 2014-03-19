using Femah.Core.Api;

namespace Femah.Core.Tests
{
    public class PutApiRequestFactory
    {
        private string _body = string.Empty;
        private string _switchName = string.Empty;
        private ApiRequest.ApiService _service = ApiRequest.ApiService.featureswitch;

        public PutApiRequestFactory ForServiceType(ApiRequest.ApiService service)
        {
            _service = service;
            return this;
        }

        public PutApiRequestFactory WithSwitchName(string name)
        {
            _switchName = name;
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
                Parameter = _switchName,
                Body = _body
            };
        }
    }
}