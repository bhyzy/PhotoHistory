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
		public ActionResult UnrecognizedCall()
		{
			throw new Exception( "404: unrecognized call" );
		}
	}
}