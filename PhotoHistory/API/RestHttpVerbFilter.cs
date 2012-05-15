using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotoHistory.API
{
	public class RestHttpVerbFilter : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var httpMethod = filterContext.HttpContext.Request.HttpMethod;
			filterContext.ActionParameters["httpVerb"] = httpMethod;
			base.OnActionExecuting( filterContext );
		}
	}

}