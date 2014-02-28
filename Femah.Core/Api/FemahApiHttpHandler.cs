using System.Net;
using System.Text;
using System.Web;

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
            ApiRequest apiRequest;
            using (var apiRequestBuilder = new ApiRequestBuilder())
            {
                apiRequest = apiRequestBuilder.Build(context.Request);
            }
            
            var apiResponse = new ApiResponse();

            if (!string.IsNullOrEmpty(apiRequest.ErrorMessage))
            {
                using (var apiResponseBuilder = new ApiResponseBuilder())
                {
                    apiResponse = apiResponseBuilder.WithBody(
                            string.Format(apiRequest.ErrorMessage))
                            .WithHttpStatusCode(apiRequest.ErrorMessageHttpStatusCode);
                }
            }
            else
            {
                switch (apiRequest.HttpMethod)
                {
                    case "GET":
                        apiResponse = ProcessApiRequest.ProcessGetRequest(apiRequest);
                        break;
                    case "PUT":
                        apiResponse = ProcessApiRequest.ProcessPutRequest(apiRequest);
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
    }
}