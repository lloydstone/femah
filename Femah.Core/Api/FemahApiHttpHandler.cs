using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
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
            
            string jsonResponse = string.Empty;
            ApiRequest apiRequest = BuildApiRequest(context.Request);
            if (!string.IsNullOrEmpty(apiRequest.ErrorMessage))
            {
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                jsonResponse = apiRequest.ErrorMessage;
                context.Response.Write(jsonResponse);
            }
            else
            {
                switch (apiRequest.HttpMethod)
                {
                    case "GET":
                        jsonResponse = ProcessApiGetRequest(context, apiRequest);
                        break;
                    case "PUT":
                        jsonResponse = ProcessApiPutRequest(context, apiRequest);
                        break;
                }

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    context.Response.Write(jsonResponse);
                }
                else
                {
                    context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                    context.Response.Write("<h1>FEMAH API</h1>");
                }
            }

        }

        private static string ProcessApiGetRequest(HttpContextBase context, ApiRequest apiRequest)
        {
            string jsonResponse;
            if (string.IsNullOrEmpty(apiRequest.Member))
            {
                //GET: http://example.com/femah.axd/api/featureswitches - Lists all Feature Switches FeatureSwitching.AllFeatures()
                //GET: http://example.com/femah.axd/api/featureswitchtypes - Lists all Feature Switch Types FeatureSwitching.AllSwitchTypes()

                switch (apiRequest.Collection)
                {
                    case ApiRequest.ApiCollection.FeatureSwitchTypes:

                        var switchTypes = Femah.AllSwitchTypes();
                        if (switchTypes != null)
                        {
                            try
                            {
                                jsonResponse = switchTypes.ToJson();
                            }
                            catch (Exception)
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                throw;
                            }
                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                            return jsonResponse;
                        }
                        
                        context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                        jsonResponse = string.Format("{{\"Error\":\"No member named: '{0}' found in: '{1}' collection\"}}",
                            apiRequest.Member, apiRequest.Collection);
                        return jsonResponse;

                    default:
                        context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                        jsonResponse = string.Format("{{\"Error\":\"Unknown collection attribute in url: '{0}' \"}}",
                            apiRequest.Collection);
                        return jsonResponse;
                }

            }

            //GET: http://example.com/femah.axd/api/featureswitch/improvedcheckout - Retrieve the Feature Switch ImprovedCheckout with it's asscoiated Feature Switch Type FeatureSwitching.GetFeature()

            switch (apiRequest.Collection)
            {
                case ApiRequest.ApiCollection.FeatureSwitch:

                    var featureSwitch = Femah.GetFeature(apiRequest.Member);
                    if (featureSwitch != null)
                    {
                        try
                        {
                            jsonResponse = featureSwitch.ToJson();
                        }
                        catch (Exception)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            throw;
                        }
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        return jsonResponse;
                    }
                    context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                    jsonResponse = string.Format("{{\"Error\":\"No member named: '{0}' found in: '{1}' collection\"}}",
                        apiRequest.Member, apiRequest.Collection);
                    return jsonResponse;

                default:
                    context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                    jsonResponse = string.Format("{{\"Error\":\"Unknown collection attribute in url: '{0}' \"}}",
                        apiRequest.Collection);
                    return jsonResponse;
            }
        }

        private static string ProcessApiPutRequest(HttpContextBase context, ApiRequest apiRequest)
        {
            return string.Empty;
        }

        public static T Deserialise<T>(string json)
        {
            var obj = Activator.CreateInstance<T>();
            using (var memoryStream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(obj.GetType());
                obj = (T) serializer.ReadObject(memoryStream);
                return obj;
            }


        }

        private static ApiRequest BuildApiRequest(HttpRequestBase request)
        {
            var apiRequest = new ApiRequest();
            if (request.Url == null)
            {
                apiRequest.ErrorMessage =
                    "{{\"Error\":\"Uri does not match expected format /femah.axd/api/[collection]/[member]\"}}";
                return apiRequest;
            }

            var uriSegments = request.Url.Segments;
            
            try
            {
                apiRequest.HttpMethod = request.HttpMethod;

                //Retrieve the collection passed in on the Url, collection is mandatory so any failure here is thrown back as an HTTP 500.
                string collection = uriSegments[3].ToLower().Replace("/", "");
                
                ApiRequest.ApiCollection apiCollection;
                if (Enum.TryParse(collection, true, out apiCollection))
                {
                    if (Enum.IsDefined(typeof (ApiRequest.ApiCollection), apiCollection))
                        apiRequest.Collection = apiCollection;
                }
                else
                {
                    apiRequest.ErrorMessage = string.Format("{{\"Error\":\"{0} is not a valid API collection object\"}}", collection);
                    return apiRequest;
                }


                if (uriSegments.Count() >= 5)
                {
                    //Retrieve the member of the collection passed in on the Url, this is optional so handle this a little better.
                    string member = uriSegments[4].ToLower().Replace("/", "");
                    apiRequest.Member = member;
                }
                
            }
            catch (IndexOutOfRangeException)
            {
                apiRequest.ErrorMessage = "{{\"Error\":\"Uri does not match expected format /femah.axd/api/[collection]/[member]\"}}";
            }

            return apiRequest;
            
        }
    }
}