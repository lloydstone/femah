using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Femah.Core.ExtensionMethods;

namespace Femah.Core.Api
{
    public class ApiRequestBuilder : IDisposable
    {
        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {

        }

        #endregion

        public ApiRequest Build(HttpRequestBase request)
        {
            var apiRequest = new ApiRequest();
            if (request.HttpMethod == "PUT" && request.ContentType != "application/json")
            {
                apiRequest.ErrorMessage = string.Format("Error: Content-Type '{0}' of request is not supported, expecting 'application/json'.", request.ContentType);
                apiRequest.ErrorMessageHttpStatusCode = HttpStatusCode.UnsupportedMediaType;
                return apiRequest;
            }

            if (request.Url == null)
            {
                apiRequest.ErrorMessage = string.Format("Error: The requested Url '{0}' is null.", request.Url);
                apiRequest.ErrorMessageHttpStatusCode = HttpStatusCode.InternalServerError;
                return apiRequest;
            }

            var uriSegments = request.Url.Segments;

            try
            {
                apiRequest.HttpMethod = request.HttpMethod;

                //If we have a request body then retrieve it from the InputStream.
                if (request.InputStream != null)
                {
                    string requestBody;
                    using (var stream = new MemoryStream())
                    {
                        request.InputStream.Seek(0, SeekOrigin.Begin);
                        request.InputStream.CopyTo(stream);
                        requestBody = Encoding.UTF8.GetString(stream.ToArray());
                    }
                    apiRequest.Body = requestBody;
                }

                if (uriSegments.Count() < 4 || uriSegments.Count()> 5)
                {
                    apiRequest.ErrorMessage = string.Format("Error: The requested Url '{0}' does not match the expected format /femah.axd/api/[service]/[parameter].", request.Url);
                    apiRequest.ErrorMessageHttpStatusCode = HttpStatusCode.InternalServerError;
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
                apiRequest.ErrorMessageHttpStatusCode = HttpStatusCode.InternalServerError;
            }

            return apiRequest;
        }
    }
}