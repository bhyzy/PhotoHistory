using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhotoHistory.Data;
using PhotoHistory.Models;
using PhotoHistory.API.Common;
using PhotoHistory.API.Authentication;
using System.Drawing;
using PhotoHistory.Common;

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
			if ( !albums.IsUserAuthorizedToViewAlbum( album, authUser, false ) )
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

		[BasicAuthorize]
		[HttpPost]
		public ActionResult UploadNew(API.Models.NewPhotoModel photo)
		{
			if ( !User.Identity.IsAuthenticated )
				throw new Exception( "user not authenticated" );
			UserModel authUser = new UserRepository().GetByUsername( User.Identity.Name );

			// # Validate request

			// photo.AlbumID: album has to exist, has to be owned by authenticated user
			AlbumRepository albums = new AlbumRepository();
			AlbumModel album = albums.GetById( photo.AlbumID, withUser: true );
			if ( album == null )
				throw new Exception( "album with specified ID not found" );
			if ( album.User != authUser )
				throw new Exception( "authenticated user is not the owner of the specified album" );

			// photo.Date: can't be set in the future
			if ( photo.Date == null )
				throw new Exception( "date of the photo hasn't been specified" );
			if ( photo.Date > DateTime.Now )
				throw new Exception( "date of the photo can't be set in the future" );

			// photo.Description: can't be longer than 1000 characters
			if ( photo.Description != null && photo.Description.Length > 1000 )
				throw new Exception( "description of the photo can't be longer that 1000 characters" );

			// photo.Image: has to be valid base-64 encoded binary file, has to be valid image
			Image image;
			try
			{
				image = ApiHelpers.Base64ToImage( photo.Image.Replace("\n", "") );
			}
			catch (System.Exception ex)
			{
				throw new Exception( "provided image file is invalid: " + ex.Message );
			}

			string path;
			double? locLatitude;
			double? locLongitude;

			using ( image )
			{
				// photo.Image: has to be a valid JPEG
				if ( !image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg) )
					throw new Exception( "provided image file is not of JPEG format" );

				// # New photo successfully validated - process

				// generate path for the image
				string photoName = "photo_" + DateTime.Now.ToString( "yyyyMMddHHmmssff" );
				path = FileHelper.getPhotoPathWithoutExtension( album, photoName ) + ".jpg";
				if ( string.IsNullOrEmpty( path ) )
					throw new Exception( "failed to generate path for the image" );

				// save the photo to the file system
				try
				{
					path = FileHelper.SavePhoto( image, album, photoName );
				}
				catch ( System.Exception ex )
				{
					throw new Exception( "failed to save the image to the file system: " + ex.Message );
				}
				if ( string.IsNullOrEmpty( path ) )
					throw new Exception( "failed to save the image to the file system" );

				// try to read GPS localization data
				locLatitude = image.GPSLatitude();
				locLongitude = image.GPSLongitude();
			}

			// create photo entity
			PhotoModel newPhoto = new PhotoModel()
			{
				Path = path,
				Date = photo.Date,
				Description = photo.Description,
				Album = album
			};
			if ( locLatitude.HasValue && locLongitude.HasValue )
			{
				newPhoto.LocationLatitude = locLatitude.Value;
				newPhoto.LocationLongitude = locLongitude.Value;
			}

			// save the entity to the database
			try
			{
				new PhotoRepository().Create( newPhoto );
			}
			catch (System.Exception ex)
			{
				// TODO: delete both the image and its thumbnail from the file system
				throw new Exception( "failed to save the photo to the database: " + ex.Message );
			}
			
			// return result
			return Json( new
			{
				ok = true,
				data = new
				{
					photo = string.Format( "{0}/api/photos/{1}", Helpers.BaseURL(), newPhoto.Id )
				}
			} );
		}
	}
}