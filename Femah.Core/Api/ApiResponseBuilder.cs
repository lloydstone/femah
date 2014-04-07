using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Femah.Core.ExtensionMethods;
using Newtonsoft.Json;

namespace Femah.Core.Api
{
    public class ApiResponseBuilder : IDisposable
    {
        private IEnumerable<Type> _featureSwitchTypes;
        private IEnumerable<IFeatureSwitch> _featureSwitches;
        private string _body;
        private HttpStatusCode _httpStatusCode;
        private bool _update;

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

        public ApiResponseBuilder CreateWithUpdatedFeatureSwitch(IFeatureSwitch featureSwitch)
        {
            _update = true;
            var fs = new List<IFeatureSwitch> { featureSwitch };
            _featureSwitches = fs;
            return this;
        }

        public ApiResponseBuilder CreateWithFeatureSwitches(IEnumerable<IFeatureSwitch> featureSwitches)
        {
            _featureSwitches = featureSwitches;
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

            if (apiResponseBuilder._update)
            {
                //Do comparison and update stuffs here.
                var desiredfeatureSwitchState = apiResponseBuilder._featureSwitches.First();
                var featureSwitchName = desiredfeatureSwitchState.Name;
                var currentFeatureSwitchState = Femah.GetFeature(featureSwitchName);
                
                if (currentFeatureSwitchState.Equals(desiredfeatureSwitchState))
                {
                    //Desired state and current state of FeatureSwitch are identical
                    return SetResponseProperties(HttpStatusCode.NotModified);
                }

                //Desired state and current state of FeatureSwitch differ, we update *everything*, 
                //it's the responsibility of the client to pass the complete desired state.
                Femah.EnableFeature(featureSwitchName, desiredfeatureSwitchState.IsEnabled);
                Femah.SetSwitchType(featureSwitchName, desiredfeatureSwitchState.FeatureType);

                var attributes = desiredfeatureSwitchState.GetCustomAttributes();
                if (attributes != null)
                    Femah.SetFeatureAttributes(featureSwitchName, attributes);

                //Check that the updated and desired FeatureSwitch state are now equal
                var updatedFeatureSwitchState = Femah.GetFeature(featureSwitchName);
                
                //Updated and desired FeatureSwitch states DO match, return to client
                if (updatedFeatureSwitchState.Equals(desiredfeatureSwitchState))
                    return SetResponseProperties(updatedFeatureSwitchState.ToJson(), HttpStatusCode.OK);

                //Updated and desired FeatureSwitch states do NOT match, roll back change
                //This is probably not the best way to do this, TODO: maybe make all changes within a transaction?
                Femah.EnableFeature(featureSwitchName, currentFeatureSwitchState.IsEnabled);
                Femah.SetSwitchType(featureSwitchName, currentFeatureSwitchState.FeatureType);

                var existingAttributes = currentFeatureSwitchState.GetCustomAttributes();
                if (existingAttributes != null)
                    Femah.SetFeatureAttributes(featureSwitchName, existingAttributes);

                return SetResponseProperties("Error: There was an issue setting the desired state for FeatureSwitch, please try again.", HttpStatusCode.NotModified);
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

        private static ApiResponse SetResponseProperties(HttpStatusCode httpStatusCode)
        {
            return new ApiResponse { Body = string.Empty, HttpStatusCode = (int)httpStatusCode };
        }




    }
}