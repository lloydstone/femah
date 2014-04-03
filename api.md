---
layout: layout
title: "Api"
credits: https://haveibeenpwned.com/API/v2
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

[test demo site by clicking here](http://femah-additionator.azurewebsites.net/femah.axd/api/featureswitch/flipflop)

```GET http://<hosting_site>/femah.axd/api/featureswitch/{name}```

<iframe width="100%" height="475" src="http://dotnetfiddle.net/Widget/6hYcyU" frameborder="0"></iframe>

## <a name="GetAllFeatureSwitchTypes"></a>Getting all initialised feature switch types
A "featureswitchtype" is an instance of an initiliased Femah feature switch type, providing specific feature switching logic. Femah ships with a number of [bundled feature switch types](http://github.com/lloydstone/femah/Femah.Core/FeatureSwitchTypes).
Returns the details (including the name, .NET type, description and configuration instructions) of each initialised featureswitchtype within the hosting applications domain.

[test demo site by clicking here](http://femah-additionator.azurewebsites.net/femah.axd/api/featureswitchtypes)

```GET http://<hosting_site>/femah.axd/api/featureswitchtypes```

## <a name="UpdateAFeatureSwitch"></a>Updating an existing initiliased feature switch
Expected to be one of the most popular uses of the API, update an existing initialised featureswitch, [be sure to create it first](userguide.md). The API requires a single featureswitchtype "name" parameter to be passed, along with a JSON body representing the desired state of the featureswitchtype.

```PUT http://<hosting_site>/femah.axd/api/featureswitch/{name}```

