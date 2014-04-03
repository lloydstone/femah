using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace Femah.Core.Api
{
    public static class ProcessApiRequest
    {
        /// <summary>
        /// Responsible for processing all HTTP GET requests in to the femah API. Receives an ApiRequest object, orchestrates and hands off to 
        /// the ApiResponseBuilder class to create the ApiResponse object.
        /// </summary>
        /// <param name="apiRequest" type="ApiRequest">A custom request object built predominantly from the http context request object, contains everything we need to route the API request appropriately.</param>
        /// <returns type="ApiResponse">The complete response to the GET request from the API, including body and HTTP status code.</returns>
        public static ApiResponse ProcessGetRequest(ApiRequest apiRequest)
        {
            using (var apiResponseBuilder = new ApiResponseBuilder())
            {
                //Example GET: http://example.com/femah.axd/api/featureswitches - Lists all Feature Switches FeatureSwitching.AllFeatures()
                //Example GET: http://example.com/femah.axd/api/featureswitches/improvedcheckout - Retrieve the Feature Switch ImprovedCheckout
                //Example GET: http://example.com/femah.axd/api/featureswitchtypes - Lists all Feature Switch Types FeatureSwitching.AllSwitchTypes()
                ApiResponse response;
                switch (apiRequest.Service)
                {
                    case ApiRequest.ApiService.featureswitches:
                        if (string.IsNullOrEmpty(apiRequest.Parameter))
                            response = apiResponseBuilder.CreateWithFeatureSwitches(Femah.AllFeatures());
                        else
                        {
                            var featureSwitch = Femah.GetFeature(apiRequest.Parameter);
                            if (featureSwitch != null)
                            {
                                response = apiResponseBuilder.CreateWithFeatureSwitches(featureSwitch);
                                break;
                            }
                            response = apiResponseBuilder.WithBody(String.Empty).WithHttpStatusCode(HttpStatusCode.NotFound);
                        }
                        break;
                    case ApiRequest.ApiService.featureswitchtypes:
                        if (!string.IsNullOrEmpty(apiRequest.Parameter))
                        {
                            response = apiResponseBuilder.WithBody(
                                String.Format("Error: Service '{0}' does not support parameter querying.", apiRequest.Service))
                                .WithHttpStatusCode(HttpStatusCode.MethodNotAllowed);
                            break;
                        }

                        response = apiResponseBuilder.CreateWithFeatureSwitchTypes(Femah.AllSwitchTypes());
                        break;
                    default:
                        response = apiResponseBuilder.WithBody(
                            String.Format("Error: '{0}' is not a valid Femah API service.", apiRequest.Service))
                            .WithHttpStatusCode(HttpStatusCode.MethodNotAllowed);
                        break;
                }
                return response;
            }
        }

        /// <summary>
        /// Responsible for processing all HTTP PUT requests in to the femah API. Receives an ApiRequest object, orchestrates and hands off to 
        /// the ApiResponseBuilder class to create the ApiResponse object.
        /// </summary>
        /// <param name="apiRequest" type="ApiRequest">A custom request object built predominantly from the http context request object, contains everything we need to route the API request appropriately.</param>
        /// <returns type="ApiResponse">The complete response to the PUT request from the API, including body and HTTP status code.</returns>
        public static ApiResponse ProcessPutRequest(ApiRequest apiRequest)
        {
            using (var apiResponseBuilder = new ApiResponseBuilder())
            {
                ApiResponse response = null;

                if (String.IsNullOrEmpty(apiRequest.Parameter))
                {
                    response = apiResponseBuilder.WithBody(
                            String.Format("Error: Service '{0}' requires a parameter to be passed. Url must match the format /femah.axd/api/[service]/[parameter].", apiRequest.Service))
                            .WithHttpStatusCode(HttpStatusCode.MethodNotAllowed);
                    return response;
                }
                switch (apiRequest.Service)
                {
                    //Example PUT: http://example.com/femah.axd/api/featureswitches/TestFeatureSwitch1 - Update a single Feature Switch (IsEnabled, SetCustomAttributes, FeatureType) 
                    case ApiRequest.ApiService.featureswitches:
                        var featureSwitch = DeSerialiseJsonBodyToFeatureSwitch(apiRequest.Body);
                        if (featureSwitch != null)
                            response = apiResponseBuilder.CreateWithUpdatedFeatureSwitch(featureSwitch);
                        else
                        {
                            response = apiResponseBuilder.WithBody("Error: Unable to deserialise the request body.  Either the JSON is invalid or the supplied 'FeatureType' value is incorrect, have you used the AssemblyQualifiedName as the 'FeatureType' in the request?")
                            .WithHttpStatusCode(HttpStatusCode.BadRequest);
                        }

                        break;
                    default:
                        response = apiResponseBuilder.WithBody(
                            String.Format("Error: Service '{0}' does not support parameter querying.", apiRequest.Service))
                            .WithHttpStatusCode(HttpStatusCode.MethodNotAllowed);
                        break;
                }
                return response;
            }
        }

        /// <summary>
        /// A helper method to deserialise a supplied JSON string to its pre-serialised concrete instance of <type>IFeatureSwitch</type>.
        /// </summary>
        /// <param name="requestBody" type="string">A JSON string representing a serialised instance of <type>IFeatureSwitch</type>.</param>
        /// <returns type="IFeatureSwitch">A deserialised concrete instance of <type>IFeatureSwitch</type> or null if there is an error deserialising the JSON string.</returns>
        private static IFeatureSwitch DeSerialiseJsonBodyToFeatureSwitch(string requestBody)
        {
            //Get the AssemblyQualifiedName from the 'FeatureType' property of the passed in JSON string.
            using (JsonReader jsonReader = new JsonTextReader(new StringReader(requestBody)))
            {
                string featureType = string.Empty;

                try
                {
                    while (jsonReader.Read())
                    {
                        if (jsonReader.TokenType == JsonToken.PropertyName && jsonReader.Value.ToString() == "FeatureType")
                        {
                            jsonReader.Read();
                            featureType = jsonReader.Value.ToString();
                            break;
                        }
                    }
                }
                catch (JsonReaderException)
                {
                    return null;
                }
                

                if (string.IsNullOrEmpty(featureType))
                    return null;

                //Recommend using AssemblyQualifiedName in API requests, allowing us to Deserialise feature switch types from external assemblies.
                Type theType = Type.GetType(featureType);
                if (theType == null)
                    return null;

                try
                {
                    var featureSwitch = JsonConvert.DeserializeObject(requestBody, theType);
                    return (IFeatureSwitch)featureSwitch;
                }
                catch (Exception)
                {
                    return null;
                }
                
            }
        }
        
    }
}