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
	[HandleJsonError]
	public class API_AlbumsController : Controller
	{
		[BasicAuthorize]
		public ActionResult ListAlbums()
		{
			AlbumRepository albumRepository = new AlbumRepository();
			ICollection<AlbumModel> albums = albumRepository.GetAll();

			List<string> albumURI = new List<string>();
			foreach ( AlbumModel album in albums )
			{
				albumURI.Add( string.Format( "{0}/api/albums/{1}", Helpers.BaseURL(), album.Id ) );
			}

			return Json( new
			{
				ok = true,
				data = new { albums = albumURI }
			}, JsonRequestBehavior.AllowGet );
		}
	}
}