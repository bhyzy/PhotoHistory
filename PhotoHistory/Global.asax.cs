using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using PhotoHistory.Common;
using PhotoHistory.API.Authentication;
using PhotoHistory.Scheduler;

namespace PhotoHistory
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add( new HandleErrorAttribute() );
			//filters.Add( new CultureInvariantFilterAttribute() );
		}

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute( "{resource}.axd/{*pathInfo}" );

			routes.MapRoute(
				 "Default", // Route name
				 "{controller}/{action}/{id}", // URL with parameters
				 new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);

		}

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			RegisterGlobalFilters( GlobalFilters.Filters );
			RegisterRoutes( RouteTable.Routes );
            BuildInitializer.InitializeBuild();
		}

		protected void Application_EndRequest()
		{
			// hack alert!
			if ( Context.Response.StatusCode == HttpBasicUnauthorizedResult.AUTHORIZATION_FAILED_STATUS )
			{
				Context.Response.StatusCode = 401;
			}
		}

        
        protected void Application_BeginRequest()
        {
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            Uri requestUrl = url.RequestContext.HttpContext.Request.Url;
            string link = string.Format( "{0}://{1}", requestUrl.Scheme, requestUrl.Authority);
            //Scheduler.SchedulerManager.InitScheduler(link); scheduler wylaczony
        }
	}
}