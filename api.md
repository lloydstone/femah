---
layout: layout
title: "Api"
credits: https://haveibeenpwned.com/API/v2;http://www.vinaysahni.com/best-practices-for-a-pragmatic-restful-api
---

The Femah API allows initialised feature switches within the hosting applications domain to be retrieved and updated via a [RESTful](http://en.wikipedia.org/wiki/Representational_State_Transfer) service. For manual configuration see [the admin UI](adminUi.md).
The API is designed to have predictable, resource orientated URL's and uses HTTP response codes to results and errors. [JSON](http://www.json.org/) is returned in all responses from the API. 

##Overview##

##Index##
* [Getting all initialised feature switches](#GetAllFeatureSwitches) 
* [Getting a single initialised feature switch](#GetAFeatureSwitch)
* [Getting all initialised feature switch types](#GetAllFeatureSwitchTypes)
* [Updating an existing initiliased feature switch](#UpdateAFeatureSwitch)

## <a name="GetAllFeatureSwitches"></a> Getting all initialised feature switches
A "featureswitch" is an instance of an initiliased Femah feature switch containing its name, feature switch type and status. This method returns the details of each initialised featureswitch within the hosting applications domain.

[test demo site by clicking here ](http://femah-additionator.azurewebsites.net/femah.axd/api/featureswitch)

```GET http://<hosting_site>/femah.axd/api/featureswitch```

<iframe width="100%" height="475" src="http://dotnetfiddle.net/Widget/0SWM1V" frameborder="0"></iframe>


## <a name="GetAFeatureSwitch"></a>Getting a single initialised feature switch
Often just a single featureswitch is required, a single featureswitch can be returned by passing a single featureswitch "name" parameter.

[test demo site by clicking here](http://femah-additionator.azurewebsites.net/femah.axd/api/featureswitch/ShorterText)

```GET http://<hosting_site>/femah.axd/api/featureswitch/{name}```

<iframe width="100%" height="475" src="http://dotnetfiddle.net/Widget/6hYcyU" frameborder="0"></iframe>

## <a name="GetAllFeatureSwitchTypes"></a>Getting all initialised feature switch types
A "featureswitchtype" is an instance of an initiliased Femah feature switch type, providing specific feature switching logic. Femah ships with a number of [bundled feature switch types](http://github.com/lloydstone/femah/Femah.Core/FeatureSwitchTypes).
Returns the details (including the name, .NET type, description and configuration instructions) of each initialised featureswitchtype within the hosting applications domain.

[test demo site by clicking here](http://femah-additionator.azurewebsites.net/femah.axd/api/featureswitchtypes)

```GET http://<hosting_site>/femah.axd/api/featureswitchtypes```

<iframe width="100%" height="475" src="http://dotnetfiddle.net/Widget/Vp7A7Q" frameborder="0"></iframe>

## <a name="UpdateAFeatureSwitch"></a>Updating an existing initiliased feature switch
Expected to be one of the most popular uses of the API, update an existing initialised featureswitch, [be sure to create it first](userguide.md). The API requires a single featureswitchtype "name" parameter to be passed, along with a JSON body representing the desired state of the featureswitchtype.

```
PUT http://<hosting_site>/femah.axd/api/featureswitch/{name}
Content-Type: application/json
Content-Length: 372
{\"IsEnabled\":false,\"Name\":\"ShorterText\",\"FeatureType\":\"Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}
```

### C&#35;

<iframe width="100%" height="475" src="http://dotnetfiddle.net/Widget/oRcFNV" frameborder="0"></iframe>

### cURL
The below is tested using cURL on Windows, installed using [Chocolatey](http://chocolatey.org/packages/curl) i.e.  `C:\>cinst curl`

####Request

```bash
C:\>curl -v -X PUT -H "Content-Type: application/json" -d "{\"IsEnabled\":false,\"Name\":\"ShorterText\",\"FeatureType\":\"Femah.Core.FeatureSwitchTypes.SimpleFeatureSwitch, Femah.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null\",\"Description\":\"Define a short description of the feature switch type here.\",\"ConfigurationInstructions\":\"Add configuration context and instructions to be displayed in the admin UI\"}" http://femah-additionator.azurewebsites.net/femah.axd/api/featureswitch/ShorterText 
```

####Response

```bash
* About to connect() to femah-additionator.azurewebsites.net port 80 (#0)
*   Trying 191.235.160.13...
* connected
* Connected to femah-additionator.azurewebsites.net (191.235.160.13) port 80 (#0
)
> PUT /femah.axd/api/featureswitch/ShorterText HTTP/1.1
> User-Agent: curl/7.28.1
> Host: femah-additionator.azurewebsites.net
> Accept: */*
> Content-Type: application/json
> Content-Length: 353
>
* upload completely sent off: 353 out of 353 bytes
< HTTP/1.1 304 Not Modified
< Cache-Control: private
< Content-Type: application/json; charset=utf-8
< Server: Microsoft-IIS/8.0
< X-AspNet-Version: 4.0.30319
< X-Powered-By: ASP.NET
< Date: Thu, 03 Apr 2014 15:14:17 GMT
< via: HTTP/1.1 proxy52
< Connection: Keep-Alive
< Set-Cookie: ARRAffinity=0cd5c45927b8089e06388be3876bfeb85472d6f2949cab977c0c102c960896a6;Path=/;Domain=femah-additionator.azurewebsites.net
* Connection #0 to host femah-additionator.azurewebsites.net left intact
* Closing connection #0
