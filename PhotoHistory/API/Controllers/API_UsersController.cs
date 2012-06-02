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
	public class API_UsersController : Controller
	{
		public ActionResult VerifyCredentials()
		{
			string[] credentials = ApiHelpers.ParseBasicAuthHeader(
				HttpContext.Request.Headers["Authorization"] );
			if ( credentials == null )
				throw new Exception( "missing/malformed HTTP Basic Auth header" );

			string authResult = UserAuthentication.TryCredentials( credentials[0], credentials[1] );
			//if ( authResult != string.Empty )
			//   throw new Exception( authResult );

			return Json( new
			{
				ok = (authResult == string.Empty),
				data = authResult
			}, JsonRequestBehavior.AllowGet );
		}

		[BasicAuthorize( Optional = true )]
		public ActionResult Describe(string userName)
		{
			UserModel authUser = (User.Identity.IsAuthenticated ?
				new UserRepository().GetByUsername( User.Identity.Name ) : null);

			userName = HttpUtility.UrlDecode( userName ).Trim();
			if ( userName == string.Empty )
				throw new Exception( "empty username" );

			UserRepository users = new UserRepository();
			UserModel user = users.GetByUsernameWithAlbums( userName, withTrustedUsers:true );
			if ( user == null )
				throw new Exception( "user not found" );

			AlbumRepository albumRepository = new AlbumRepository();
			List<string> albums = new List<string>();
			foreach ( AlbumModel album in user.Albums )
			{
				if ( albumRepository.IsUserAuthorizedToViewAlbum( album, authUser ) )
				{
					albums.Add( string.Format( "{0}/api/albums/{1}", Helpers.BaseURL(), album.Id ) );
				}
			}

			return Json( new
			{
				ok = true,
				data = new
				{
					id = user.Id,
					username = user.Login,
					date_of_birth = (user.DateOfBirth.HasValue ? new
					{
						day = user.DateOfBirth.Value.Day,
						month = user.DateOfBirth.Value.Month,
						year = user.DateOfBirth.Value.Year
					} : null),
					about = user.About,
					albums = albums
				}
			}, JsonRequestBehavior.AllowGet );
		}
	}
}