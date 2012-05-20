using System.Web.Mvc;

namespace PhotoHistory.API.Common
{
	public class HandleJsonError : ActionFilterAttribute
	{
		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if ( /* filterContext.HttpContext.Request.IsAjaxRequest() && */ filterContext.Exception != null )
			{
				filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
				filterContext.Result = new JsonResult()
				{
					JsonRequestBehavior = JsonRequestBehavior.AllowGet,
					Data = new
					{
						ok = false,
						error = filterContext.Exception.Message,
					}
				};
				filterContext.ExceptionHandled = true;
			}
		}
	}
}