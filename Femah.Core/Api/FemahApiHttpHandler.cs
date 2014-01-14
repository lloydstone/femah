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
    internal sealed class FemahApiHttpHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;

            // Great article on verbs to use in API design http://stackoverflow.com/questions/2001773/understanding-rest-verbs-error-codes-and-authentication/2022938#2022938
            
            string jsonResponse;
            ApiRequest apiRequest = BuildApiRequest(context.Request);
            if (!string.IsNullOrEmpty(apiRequest.ErrorMessage))
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                jsonResponse = apiRequest.ErrorMessage;
                context.Response.Write(jsonResponse);
            }

            else switch (apiRequest.HttpMethod)
            {
                case "GET":
                    if (string.IsNullOrEmpty(apiRequest.Member))
                    {
                        //GET: http://example.com/femah.axd/api/featureswitches - Lists all Feature Switches FeatureSwitching.AllFeatures()
                        //GET: http://example.com/femah.axd/api/featureswitchetypes - Lists all Feature Switche Types FeatureSwitching.AllSwitchTypes()
                    
                        switch (apiRequest.Collection)
                        {
                            case ApiRequest.ApiCollection.FeatureSwitchTypes:

                                var switchTypes = Femah.AllSwitchTypes();
                                if (switchTypes != null)
                                {
                                    context.Response.StatusCode = (int)HttpStatusCode.OK;

                                    //TODO: Argh, is there anyway to Json serialise a List<Type> with the DataContractJsonSerializer?
                                    //Let's not try and do that, rather, take the approach of implementing a TolerantReader http://martinfowler.com/bliki/TolerantReader.html
                                    //in the client.
                                    jsonResponse = switchTypes.ToJson();
                                }
                                else
                                {
                                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                                    jsonResponse = string.Format("{{\"Error\":\"No member named: '{0}' found in: '{1}' collection\"}}", apiRequest.Member, apiRequest.Collection);
                                }
                                break;

                            default:
                                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                                jsonResponse = string.Format("{{\"Error\":\"Unknown collection attribute in url: '{0}' \"}}", apiRequest.Collection);
                                break;

                        }

                        context.Response.Write(jsonResponse);
                    }

                    else if (!string.IsNullOrEmpty(apiRequest.Member))
                    {
                        //GET: http://example.com/femah.axd/api/featureswitch/improvedcheckout - Retrieve the Feature Switch ImprovedCheckout with it's asscoiated Feature Switch Type FeatureSwitching.GetFeature()


                        switch (apiRequest.Collection)
                        {
                            case ApiRequest.ApiCollection.FeatureSwitch:
                            
                                var featureSwitch = Femah.GetFeature(apiRequest.Member);
                                if (featureSwitch != null)
                                {
                                    //var featureSwitch = FeatureSwitching.AllFeatures();
                                    context.Response.StatusCode = (int) HttpStatusCode.OK;
                                    jsonResponse = featureSwitch.ToJson();
                                }
                                else
                                {
                                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                                    jsonResponse = string.Format("{{\"Error\":\"No member named: '{0}' found in: '{1}' collection\"}}", apiRequest.Member, apiRequest.Collection);
                                }
                                break;

                            default:
                                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                                jsonResponse = string.Format("{{\"Error\":\"Unknown collection attribute in url: '{0}' \"}}", apiRequest.Collection);
                                break;

                        }
                    
                        context.Response.Write(jsonResponse);

                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        context.Response.Write("<h1>FEMAH API (PUT)</h1>");
                    }
                    break;
                case "PUT":
                    context.Response.Write("<h1>FEMAH API (PUT)</h1>");
                    break;
                case "DELETE":
                    break;
            }

        }

        public bool IsReusable
        {
            get { return true; }
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

        private static ApiRequest BuildApiRequest(HttpRequest request)
        {
            var uriSegments = request.Url.Segments;
            var apiRequest = new ApiRequest();
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