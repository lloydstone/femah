using System;
using System.Collections.Generic;
using System.Net;
using Femah.Core.ExtensionMethods;

namespace Femah.Core.Api
{
    public class ApiResponseBuilder : IDisposable
    {
        private IEnumerable<Type> _featureSwitchTypes;
        private IEnumerable<IFeatureSwitch> _featureSwitches;
        private ApiRequest _apiRequest;
        private string _body;
        private HttpStatusCode _httpStatusCode;

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {

        }

        #endregion

        public ApiResponseBuilder CreateWithFeatureSwitchTypes(IEnumerable<Type> featureSwitchTypes)
        {
            _featureSwitchTypes = featureSwitchTypes;
            return this;
        }

        public ApiResponseBuilder CreateWithFeatureSwitches(IFeatureSwitch featureSwitch)
        {
            var fs = new List<IFeatureSwitch> {featureSwitch};
            _featureSwitches = fs;
            return this;
        }

        public ApiResponseBuilder CreateWithFeatureSwitches(IEnumerable<IFeatureSwitch> featureSwitches)
        {
            _featureSwitches = featureSwitches;
            return this;
        }

        public ApiResponseBuilder WithApiRequest(ApiRequest apiRequest)
        {
            _apiRequest = apiRequest;
            return this;
        }

        public ApiResponseBuilder WithBody(string body)
        {
            _body = body;
            return this;
        }

        public ApiResponseBuilder WithHttpStatusCode(HttpStatusCode httpStatusCode)
        {
            _httpStatusCode = httpStatusCode;
            return this;
        }

        /// <summary>
        /// An implicit operator to save us having to use somehting like .Build() at the end of the fluent chain.
        /// The heart and soul of the fluent builder, determines which methods were chained together and handles the logic to 
        /// build the ApiResponse object in its entirety.
        /// </summary>
        /// <param name="apiResponseBuilder"></param>
        /// <returns></returns>
        public static implicit operator ApiResponse(ApiResponseBuilder apiResponseBuilder)
        {

            if (apiResponseBuilder._body != null && apiResponseBuilder._httpStatusCode > 0)
            {
                //WithBody and WithHttpStatusCode defined
                return SetResponseProperties(apiResponseBuilder._body.ToJson(), apiResponseBuilder._httpStatusCode);
            }

            if (apiResponseBuilder._featureSwitchTypes != null)
            {
                //CreateWithFeatureSwitchTypes defined
                var apiFeatureSwitchTypes = new List<ApiFeatureSwitchType>();
                try
                {
                    foreach (var featureSwitchType in apiResponseBuilder._featureSwitchTypes)
                    {
                        var featureSwitchTypeInstance = (IFeatureSwitch) Activator.CreateInstance(featureSwitchType);
                        var apiFeatureSwitchType = new ApiFeatureSwitchType
                        {
                            FeatureSwitchType = featureSwitchType,
                            ConfigurationInstructions = featureSwitchTypeInstance.ConfigurationInstructions,
                            Description = featureSwitchTypeInstance.Description,
                            Name = featureSwitchType.Name
                        };
                        apiFeatureSwitchTypes.Add(apiFeatureSwitchType);
                    }

                    return SetResponseProperties(apiFeatureSwitchTypes.ToJson(), HttpStatusCode.OK);
                }
                catch (Exception exception)
                {
                    return SetResponseProperties(exception.InnerException.ToJson(), HttpStatusCode.InternalServerError);
                }
            }

            if (apiResponseBuilder._featureSwitches != null)
            {
                //CreateWithFeatureSwitches defined
                try
                {
                    return SetResponseProperties(apiResponseBuilder._featureSwitches.ToJson(), HttpStatusCode.OK);
                }
                catch (Exception exception)
                {
                    return SetResponseProperties(exception.InnerException.ToJson(), HttpStatusCode.InternalServerError);
                }
            }

            return SetResponseProperties(string.Empty, HttpStatusCode.NotFound);
        }

        /// <summary>
        /// A small helper method to make it a little nicer to new up an ApiResponse object.
        /// </summary>
        /// <param name="body" type="string">The ApiResponse body, expected to be JSON serialised.</param>
        /// <param name="httpStatusCode" type="HttpStatusCode">The HTTP StatusCode we want to set within the ApiResponse.</param>
        /// <returns></returns>
        private static ApiResponse SetResponseProperties(string body, HttpStatusCode httpStatusCode)
        {
            //TODO: Use JSON.NET to validate if the Body is JSON??
            return new ApiResponse { Body = body, HttpStatusCode = (int)httpStatusCode };
        }


    }
}