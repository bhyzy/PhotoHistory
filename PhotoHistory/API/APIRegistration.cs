using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotoHistory.API
{
	public class APIRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get { return "API"; }
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
				"API list albums",
				"api/albums",
				new { controller = "API_Albums", action = "ListAlbums" } );

			context.MapRoute(
				"API default",
				"api/*",
				new { controller = "API_API", action = "UnrecognizedCall" } );
		}
	}
}