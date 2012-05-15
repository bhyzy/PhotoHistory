using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotoHistory.API.Controllers
{
	public class API_APIController : Controller
	{
		public ActionResult UnrecognizedCall()
		{
			return new HttpNotFoundResult("Unrecognized call");
		}
	}
}