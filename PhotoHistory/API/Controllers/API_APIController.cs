using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhotoHistory.API.Common;

namespace PhotoHistory.API.Controllers
{
	[HandleJsonError]
	public class API_APIController : Controller
	{
		public ActionResult Hello()
		{
			return Json( new { 
				ok = true, 
				data = "Hello! Everything's working fine :)" 
				}, 
				JsonRequestBehavior.AllowGet );
		}
	}
}