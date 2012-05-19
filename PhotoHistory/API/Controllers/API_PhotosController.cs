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
	public class API_PhotosController : Controller
	{
		[BasicAuthorize( Optional = true )]
		public ActionResult Describe(int id)
		{
			UserModel authUser = (User.Identity.IsAuthenticated ?
				new UserRepository().GetByUsername( User.Identity.Name ) : null);

			// check if photo with given id exists
			PhotoRepository photos = new PhotoRepository();
			PhotoModel photo = photos.GetById( id, withAlbum: true );
			if ( photo == null )
				throw new Exception( "photo not found" );

			// check if the caller can access the album containing the photo
			AlbumRepository albums = new AlbumRepository();
			AlbumModel album = albums.GetById( photo.Album.Id, withUser: true, withTrustedUsers: true );
			if ( !albums.IsUserAuthorizedToViewAlbum( album, authUser ) )
				throw new Exception( "user not authorized to view this photo/album" );

			// send back the photo information
			return Json( new
			{
				ok = true,
				data = new
				{
					id = photo.Id,
					album = string.Format( "{0}/api/albums/{1}", Helpers.BaseURL(), album.Id ),
					date = new
					{
						day = photo.Date.Day,
						month = photo.Date.Month,
						year = photo.Date.Year
					},
					description = photo.Description,
					image = Helpers.BaseURL() + photo.Path,
					thumbnail = Helpers.BaseURL() + photo.Path.Substring(0, photo.Path.LastIndexOf(".jpg")) + "_mini.jpg",
					latitude = photo.LocationLatitude,
					longitude = photo.LocationLongitude
				}
			}, JsonRequestBehavior.AllowGet );
		}
	}
}