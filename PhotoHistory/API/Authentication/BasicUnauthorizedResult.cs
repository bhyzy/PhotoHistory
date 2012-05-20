using System.Web.Mvc;
using System;

namespace PhotoHistory.API.Authentication
{
	public class HttpBasicUnauthorizedResult : JsonResult
	{
		public static int AUTHORIZATION_FAILED_STATUS = 666; // hack alert!

		public HttpBasicUnauthorizedResult() : base() 
		{
			Data = new
			{
				ok = false,
				error = "unauthorized access, please authenticate"
			};
			JsonRequestBehavior = JsonRequestBehavior.AllowGet;
		}

		public override void ExecuteResult(ControllerContext context)
		{
			if ( context == null ) throw new ArgumentNullException( "context" );
			base.ExecuteResult( context );
			context.HttpContext.Response.StatusCode = AUTHORIZATION_FAILED_STATUS;
		}
	}
}