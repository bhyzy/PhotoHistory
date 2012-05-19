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
		[BasicAuthorize( Optional = true )]
		public ActionResult Describe(int id)
		{
			UserModel authUser = (User.Identity.IsAuthenticated ?
				new UserRepository().GetByUsername( User.Identity.Name ) : null);

			// check if album with given id exists
			AlbumRepository albums = new AlbumRepository();
			AlbumModel album = albums.GetById( id, 
				withUser: true, withPhotos: true, withComments: true, withCategory: true, withTrustedUsers: true );
			if ( album == null )
				throw new Exception( "album not found" );

			// check if the caller can access the album
			if ( !albums.IsUserAuthorizedToViewAlbum( album, authUser ) )
				throw new Exception( "user not authorized to view this album" );

			// prepare photos list
			List<string> photos = new List<string>();
			foreach ( PhotoModel photo in album.Photos )
			{
				photos.Add( string.Format( "{0}/api/photos/{1}", Helpers.BaseURL(), photo.Id ) );
			}

			// prepare comments list
			List<Object> comments = new List<Object>();
			foreach ( CommentModel comment in album.Comments )
			{
				comments.Add( new
				{
					author = comment.User.Login,
					date = new
					{
						day = comment.Date.Day,
						month = comment.Date.Month,
						year = comment.Date.Year
					},
					body = comment.Body
				} );
			}

			// send back the album information
			return Json( new
			{
				ok = true,
				data = new
				{
					id = album.Id,
					name = album.Name,
					description = album.Description,
					category = album.Category.Name,
					owner = album.User.Login,
					is_public = album.Public,
					rating = album.Rating,
					views = album.Views,
					photos = photos,
					comments = comments
				}
			}, JsonRequestBehavior.AllowGet );
		}
	}
}