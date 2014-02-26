using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Femah.Core.ExtensionMethods;

namespace Femah.Core.Api
{
    public class FemahApiHttpHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(new HttpContextWrapper(context));
        }

        public void ProcessRequest(HttpContextBase context)
        {
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;

            // Great article on verbs to use in API design http://stackoverflow.com/questions/2001773/understanding-rest-verbs-error-codes-and-authentication/2022938#2022938
            
            var apiRequest = BuildApiRequest(context.Request);
            var apiResponse = new ApiResponse();

            if (!string.IsNullOrEmpty(apiRequest.ErrorMessage))
            {
                using (var apiResponseBuilder = new ApiResponseBuilder())
                {
                    apiResponse = apiResponseBuilder.WithBody(
                            string.Format(apiRequest.ErrorMessage))
                            .WithHttpStatusCode(HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                switch (apiRequest.HttpMethod)
                {
                    case "GET":
                        apiResponse = ProcessApiGetRequest(context, apiRequest);
                        break;
                    case "PUT":
                        apiResponse = ProcessApiPutRequest(context, apiRequest);
                        break;
                }

                if (apiResponse == null)
                {
                    using (var apiResponseBuilder = new ApiResponseBuilder())
                    {
                        apiResponse = apiResponseBuilder.WithBody(
                                string.Format("Error: API GET request returned NULL"))
                                .WithHttpStatusCode(HttpStatusCode.InternalServerError);
                    }
                }
            }

            context.Response.StatusCode = apiResponse.HttpStatusCode;
            context.Response.Write(apiResponse.Body);
        }

        /// <summary>
        /// Responsible for processing all HTTP GET requests in to the femah API. Orchestrates and hands off to appropriate builders 
        /// to create request and response objects.
        /// </summary>
        /// <param name="context" type="HttpContextBase">The current http context, required to allow us to write the API response back on the response object.</param>
        /// <param name="apiRequest" type="ApiRequest">A custom request object built predominantly from the http context request object, contains everything we need to route the API request appropriately.</param>
        /// <returns type="ApiResponse">The complete response to the GET request from the API, including body and HTTP status code.</returns>
        private static ApiResponse ProcessApiGetRequest(HttpContextBase context, ApiRequest apiRequest)
        {
            using (var apiResponseBuilder = new ApiResponseBuilder())
            {
                ApiResponse response;
                if (string.IsNullOrEmpty(apiRequest.Parameter))
                {
                    //Example GET: http://example.com/femah.axd/api/featureswitches - Lists all Feature Switches FeatureSwitching.AllFeatures()
                    //Example GET: http://example.com/femah.axd/api/featureswitchtypes - Lists all Feature Switch Types FeatureSwitching.AllSwitchTypes()

                    switch (apiRequest.Service)
                    {
                        case ApiRequest.ApiService.featureswitches:
                            response = apiResponseBuilder.CreateWithFeatureSwitches(Femah.AllFeatures())
                                .WithApiRequest(apiRequest);
                            break;
                        case ApiRequest.ApiService.featureswitchtypes:
                            response = apiResponseBuilder.CreateWithFeatureSwitchTypes(Femah.AllSwitchTypes())
                                .WithApiRequest(apiRequest);
                            break;
                        default:
                            response = apiResponseBuilder.WithBody(string.Empty)
                                .WithHttpStatusCode(HttpStatusCode.NotFound);
                            break;
                    }
                    return response;
                }

                //Example GET: http://example.com/femah.axd/api/featureswitch/improvedcheckout - Retrieve the Feature Switch ImprovedCheckout
                switch (apiRequest.Service)
                {
                    case ApiRequest.ApiService.featureswitch:

                        var featureSwitch = Femah.GetFeature(apiRequest.Parameter);
                        if (featureSwitch != null)
                        {
                            response = apiResponseBuilder.CreateWithFeatureSwitches(featureSwitch)
                                .WithApiRequest(apiRequest);
                            break;
                        }
                        response = apiResponseBuilder.WithBody(string.Empty)
                                .WithHttpStatusCode(HttpStatusCode.NotFound);
                            break;
                    default:
                        response = apiResponseBuilder.WithBody(
                                string.Format("Error: Service '{0}' does not support parameter querying.", apiRequest.Service))
                                .WithHttpStatusCode(HttpStatusCode.MethodNotAllowed);
                            break;
                }
                return response;
            }
        }

        private static ApiResponse ProcessApiPutRequest(HttpContextBase context, ApiRequest apiRequest)
        {
            throw new NotImplementedException();
        }

        private static ApiRequest BuildApiRequest(HttpRequestBase request)
        {
            var apiRequest = new ApiRequest();
            if (request.Url == null)
            {
                apiRequest.ErrorMessage = string.Format("Error: The requested Url '{0}' is null.", request.Url);
                return apiRequest;
            }

            var uriSegments = request.Url.Segments;
            
            try
            {
                apiRequest.HttpMethod = request.HttpMethod;
                
                if (uriSegments.Count() < 4 || uriSegments.Count()> 5)
                {
                    apiRequest.ErrorMessage = string.Format("Error: The requested Url '{0}' does not match the expected format /femah.axd/api/[service]/[parameter].", request.Url);
                    return apiRequest;
                }

                //Retrieve the service passed in on the Url, service is mandatory.
                string service = uriSegments[3].ToLower().Replace("/", "");

                ApiRequest.ApiService apiService;
                if (EnumExtensions.TryParse(service, out apiService))
                {
                    if (Enum.IsDefined(typeof (ApiRequest.ApiService), apiService))
                        apiRequest.Service = apiService;
                }
                else
                {
                    apiRequest.Service = null;
                }

                if (uriSegments.Count() == 5)
                {
                    //Retrieve the optional parameter of the service passed in on the Url, this is optional so handle this a little better.
                    string member = uriSegments[4].ToLower().Replace("/", "");
                    apiRequest.Parameter = member;
                }

            }
            catch (IndexOutOfRangeException)
            {
                apiRequest.ErrorMessage = string.Format("Error: The requested Url '{0}' does not match the expected format /femah.axd/api/[service]/[parameter].", request.Url);
            }

            return apiRequest;
        }


    }

}