using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhotoHistory.Data;
using PhotoHistory.Models;
using PhotoHistory.API.Common;
using PhotoHistory.API.Authentication;

namespace PhotoHistory.API.Controllers
{
	public class API_UsersController : Controller
	{
		[HandleJsonError]
		public ActionResult VerifyCredentials()
		{
			string[] credentials = ApiHelpers.ParseBasicAuthHeader( 
				HttpContext.Request.Headers["Authorization"] );
			if ( credentials == null )
				throw new Exception( "missing/malformed HTTP Basic Auth header" );

			string authResult = UserAuthentication.TryCredentials( credentials[0], credentials[1] );
			if ( authResult != string.Empty )
				throw new Exception( authResult );

			return Json( new { 
				ok = true, 
				data = "credentials successfully authenticated"
			}, JsonRequestBehavior.AllowGet );
		}
	}
}