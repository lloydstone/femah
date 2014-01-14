using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Femah.Core;
using Femah.Core.Providers;
using Femah.TestWebApp.Code;

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

            // When custom configuration is required:
            //FeatureSwitching.Configure()
            //    .FeatureSwitchEnum(typeof(FemahFeatureSwitches))
            //    .AdditionalSwitchTypesFromAssembly(typeof(FiftyFiftyFeatureSwitch).Assembly)
            //    .ContextFactory(new CustomFemahContextFactory())
            //    .Initialise();

            //var provider = new SqlServerProvider();
            //provider.Configure(ConfigurationManager.ConnectionStrings["SqlServerConnection"].ConnectionString);

            //FeatureSwitching.Configure()
            //    .ContextFactory(new CustomFemahContextFactory())
            //    .AdditionalSwitchTypesFromAssembly( typeof(FiftyFiftyFeatureSwitch).Assembly )
            //    .FeatureSwitchEnum( typeof(FemahFeatureSwitches) )
            //    .Initialise();
        }
    }
}