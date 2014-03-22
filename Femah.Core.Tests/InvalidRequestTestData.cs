using System.Collections;
using System.Net;
using NUnit.Framework;

namespace Femah.Core.Tests
{
    public class InvalidRequestTestData
    {
        public static IEnumerable TestCases {
            get
            {
                yield return new TestCaseData("TestFeatureSwitch", "This is not JSON",
                    "\"Error: Unable to deserialise the request body.  Either the JSON is invalid or the supplied 'FeatureType' value is incorrect, have you used the AssemblyQualifiedName as the 'FeatureType' in the request?\"",
                    HttpStatusCode.BadRequest).SetName("GivenRequestWithInvalidJson_ThenHttpCode400AndErrorAreReturned");
                yield return new TestCaseData("TestFeatureSwitch",
                    "{{\"IsEnabled\":NotValidBoolean,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}}",
                    "\"Error: Unable to deserialise the request body.  Either the JSON is invalid or the supplied 'FeatureType' value is incorrect, have you used the AssemblyQualifiedName as the 'FeatureType' in the request?\"",
                    HttpStatusCode.BadRequest).SetName("GivenRequestWithInvalidEnabledValue_ThenHttpCode400AndErrorAreReturned");
                yield return
                    new TestCaseData("TestFeatureSwitch",
                        "{{\"IsEnabled\":true,\"Name\":\"TestFeatureSwitch1\",\"FeatureType\":\"Invalid.FeatureType.Will.Not.Deserialise\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}}",
                        "\"Error: Unable to deserialise the request body.  Either the JSON is invalid or the supplied 'FeatureType' value is incorrect, have you used the AssemblyQualifiedName as the 'FeatureType' in the request?\"",
                        HttpStatusCode.BadRequest).SetName("GivenRequestWithInvalidFeatureType_ThenHttpCode400AndErrorAreReturned"); ;
            } 
        }
    }
}