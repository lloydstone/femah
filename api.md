---
layout: layout
title: "Api"
---
<section class="content">
  <h1>{{ page.title }}</h1>

</section>

credits: https://haveibeenpwned.com/API/v2

##API##
The Femah API allows initialised feature switches within the hosting applications domain to be retrieved and updated via a RESTful service. For manual configuration see [the admin UI](adminUi.md)

##Overview##

##Index##
* Getting all initialised feature switches 
* Getting a single initialised feature switch
* Getting all initialised feature switch types
* Updating an existing initiliased feature switch

##Getting all initialised feature switches##
A "featureswitch" is an instance of an initiliased Femah feature switch containing its name, feature switch type and status. This method returns the details of each initialised featureswitch within the hosting applications domain.

[test demo site by clicking here ](http://additionator.azure.net/femah.axd/api/featureswitch)
```GET http://<hosting_site>/femah.axd/api/featureswitch```

##Getting a single initialised feature switch##
Often just a single featureswitch is required, a single featureswitch can be returned by passing a single featureswitch "name" parameter.

[test demo site by clicking here](http://additionator.azure.net/femah.axd/api/featureswitch/flipflop)
```GET http://<hosting_site>/femah.axd/api/featureswitch/{name}```

##Getting all initialised feature switch types##
A "featureswitchtype" is an instance of an initiliased Femah feature switch type, providing specific feature switching logic. Femah ships with a number of [bundled feature switch types](http://github.com/lloydstone/femah/Femah.Core/FeatureSwitchTypes).
Returns the details (including the name, .NET type, description and configuration instructions) of each initialised featureswitchtype within the hosting applications domain.

[test demo site by clicking here](http://additionator.azure.net/femah.axd/api/featureswitchtypes)
```GET http://<hosting_site>/femah.axd/api/featureswitchtypes```

##Updating an existing initiliased feature switch##
Expected to be one of the most popular uses of the API, update an existing initialised featureswitch, [be sure to create it first](userguide.md). The API requires a single featureswitchtype "name" parameter to be passed, along with a JSON body representing the desired state of the featureswitchtype.

```PUT http://<hosting_site>/femah.axd/api/featureswitch/{name}```

