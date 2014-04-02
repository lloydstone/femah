<a href="https://github.com/lloydstone/femah"><img src="https://f.cloud.github.com/assets/362197/2124054/b67ee748-9237-11e3-8b44-ff2bd7fba50c.png" alt="Femah project home logo"/></a>

<a href="http://teamcity.jetbrains.com/viewType.html?buildTypeId=NetCommunityProjects_Femah_Femah&guest=1"><img src="http://teamcity.jetbrains.com/app/rest/builds/buildType:(id:NetCommunityProjects_Femah_Femah)/statusIcon" alt="teamcity.jetbrains Femah build status"/></a>

##Introduction##
Femah is a feature switching/toggling library requiring minimal configuration.  Designed from day 1 to be enterprise scalable, our intention is to provide a robust and extensible library to enable the .NET community to embrace the [stable trunk method of development](http://paulhammant.com/2013/04/05/what-is-trunk-based-development/) and realise Continuous Delivery.

###MVP Goals###
* Provide common feature switch types for majority of typical scenarios out of the box
* Provide ability to extend with custom feature switch types
* Provide a UI for controlling feature state
* Provide a full API for automation
* Completely fail-safe, host app stability is never compromised

###Femah Enables###
* A/B style testing and canary releases
* Feature state testing in the build/deployment pipeline via the API

###Configuring Femah in your Web application###

####1. Install Femah NuGet package####
Within the target Visual Studio web application, using the Visual Studio Package Manager
```Install-Package Femah```

####2. Register the Femah HTTP handlers within your app####
Add the following to the bottom of your web applications web.config (as a child of the < configuration > element)

```
<location path="femah.axd" inheritInChildApplications="false">
    <!-- IIS 6 -->
    <system.web>
      <httpHandlers>
        <add verb="GET,PUT,DELETE" path="femah.axd/api*" type="Femah.Core.Api.FemahApiHttpHandler, Femah.Core" />
        <add verb="POST,GET,HEAD" path="femah.axd" type="Femah.Core.UI.FemahHttpHandlerFactory, Femah.Core" />
      </httpHandlers>
    </system.web>
    <!-- iis 7 -->
    <system.webServer>
      <validation validateIntegratedModeConfiguration="false" />
      <handlers>
        <add name="FEMAHAPI" verb="GET,PUT,DELETE" path="femah.axd/api*" type="Femah.Core.Api.FemahApiHttpHandler, Femah.Core" preCondition="integratedMode" />
        <add name="FEMAH" verb="POST,GET,HEAD" path="femah.axd" type="Femah.Core.UI.FemahHttpHandlerFactory, Femah.Core" preCondition="integratedMode" />
      </handlers>
    </system.webServer>
  </location>
```

####3. Initialise Femah within your app####
Initialise feature switching within the Global.asax code behind of your web application, using ```Femah.Initialise();```

Example
```
namespace Femah.TestWebApp
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Simplest way to initialise FEMAH:
            Femah.Initialise();
        }
    }
}
```
###Using Femah in your Web application###
####1. Add feature switches####
Standard (using built in Femah feature switch types) feature switches are simply initiated by adding an Enum called FemahFeatureSwitches.  Femah uses reflection to determine them at start-up.

Example
```
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	namespace Femah.TestWebApp.Code
	{
		public enum FemahFeatureSwitches
		{
			NewXmas2013Homepage,
			ImprovedCheckout
		}
	}
```
####2. Use the Femah Admin UI to enable/disable and change the type of feature switches####
* Hit F5 in your web application and browse to http://localhost/femah.axd
* Observe the Femah Admin UI and configure the feature switch type

####3. Add conditional logic within your web application to use feature switches####
To use the feature switches you need to add conditional logic within your code, this is simple using the static IsFeatureOn method.

Example given within a Razor view
```
	@using Femah.Core

	@{
		ViewBag.Title = "Index";
	}

	<h2>Index</h2>

	<p>Ordinary stuff here.</p>

	@if ( Femah.IsFeatureOn((int)FemahFeatureSwitches.ImprovedCheckout)) {
	<h3>Feature switch enabled.</h3>
	}
```

###Building Femah###
1. [Setup Git](http://help.github.com/win-set-up-git/)

1. Get Femah source by cloning the github repo

		$ git clone git@github.com:lloydstone/Femah.git

1. Build Femah (versions, compiles, executes Unit Tests and creates NuGet package) 

		C:\Femah>Start-Build.bat
		
###Contributing###
Please start with a small (very) pull request, the smaller and more focused the pull request the far higher the chance of it being merged in.

1. Check out the current [Issues](https://github.com/lloydstone/femah/issues) and start commenting

or

1. Start a conversation by raising a new issue for anything you feel we should be working on

		
### License ###
Femah is released under the [MIT license](http://opensource.org/licenses/MIT)
