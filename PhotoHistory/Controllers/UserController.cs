using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhotoHistory.Models;
using PhotoHistory.Data;
using System.Web.Security;
using PhotoHistory.Common;

namespace PhotoHistory.Controllers
{
	public class UserController : Controller
	{
		//
		// GET: /User/
        
		public ActionResult Index()
		{
            return RedirectToAction("ViewProfile");
		}
        
        public ActionResult ViewProfile(string userName)
        {
            if(string.IsNullOrEmpty(userName))
                 userName = HttpContext.User.Identity.Name;

            UserRepository repo = new UserRepository();
            UserModel user = repo.GetByUsernameWithPhotos(userName);
            UserProfileModel profile = null;

            if (user == null)
            {
                ViewBag.ErrorMessage = string.Format("Can't find user {0}", userName);
                return View(profile);
            }
            
            DateTime? startTime = user.DateOfBirth;

            string age = "";
            if (startTime == null)
                age = "Unavailable";
            else
                age = "" + Helpers.GetAge(startTime ?? DateTime.Today);

            profile = new UserProfileModel()
            {
                Name = user.Login,
                About = user.About,
                Age = age,
                Albums = new List<AlbumProfileModel>()
            };

            string start,end;

            foreach (AlbumModel album in user.Albums)
            {  
                Helpers.AlbumDateRange(album, out start , out end);

                AlbumProfileModel profileAlbum = new AlbumProfileModel()
                {
                    Id = album.Id,
                    Name = album.Name,
                    Thumbnails = Helpers.AlbumThumbnails(album),
                    StartDate = start,
                    EndDate = end,
                    Views = album.Views
                }; 
                profile.Albums.Add(profileAlbum);
            }
            profile.Albums.Sort(delegate(AlbumProfileModel a, AlbumProfileModel b)
            {
                int aa=a.Id??0;
                int bb=b.Id??0;
                return aa.CompareTo(bb);
            });
            return View(profile);
        }

		public ActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Create(NewUserModel newUser)
		{
			if ( ModelState.IsValid )
			{
				UserModel user = new UserModel()
				{
					Login = newUser.Login,
					Password = newUser.Password.HashMD5(),
					Email = newUser.Email,
					ActivationCode = (new Random().Next( 1000 )).ToString().HashMD5(),
					DateOfBirth = null,
					About = null,
					NotifyComment = true,
					NotifyPhoto = true,
					NotifySubscription = true
				};

				Uri requestUrl = Url.RequestContext.HttpContext.Request.Url;
				string activationLink = string.Format( "{0}://{1}{2}", requestUrl.Scheme, requestUrl.Authority,
					Url.Action( "Activate", "User", new { user = Url.Encode( user.Login ), token = user.ActivationCode } ) );

				try
				{
					Helpers.SendEmail( user.Email, "PastExplorer account activation",
						string.Format( "Hello, click <a href=\"{0}\">here</a> to activate your PastExplorer account.", activationLink ) );
				}
				catch ( Exception )
				{
					ViewBag.ErrorMessage = "Failed to send activation e-mail message. Please try again later.";
					return View( newUser );
				}

				UserRepository users = new UserRepository();
				users.Create( user );

				return View( "Created", user );
			}

			return View( newUser );
		}

		public ActionResult SignIn()
		{
			return View();
		}

		[HttpPost]
		public ActionResult SignIn(SignInUserModel signUser, FormCollection form, string returnUrl)
		{
			if ( ModelState.IsValid )
			{
				UserRepository userRepository = new UserRepository();
				UserModel user = userRepository.GetByUsername( signUser.Login );
				if ( user != null && user.Password == signUser.Password.HashMD5() )
				{
					if ( user.ActivationCode == null )
					{
						AuthenticateUser( signUser.Login, signUser.Remember );

						if ( !String.IsNullOrEmpty( returnUrl ) )
						{
							return Redirect( returnUrl );
						}
						else
						{
							return RedirectToAction( "Index", "Home" );
						}
					}
					else
					{
						ViewBag.ErrorMessage = "Your account hasn't been activated yet. " +
							"Please visit the activation link from the e-mail message we've sent you.";
						return View( signUser );
					}
				}
				else
				{
					ViewBag.ErrorMessage = "Specified username and/or password is incorrect.";
					return View( signUser );
				}
			}

			return View( signUser );
		}

		[Authorize]
		public ActionResult SignOut()
		{
			FormsAuthentication.SignOut();
			return RedirectToAction( "SignIn" );
		}

		public ActionResult Activate(string user, string token)
		{
			if ( !string.IsNullOrEmpty( user ) && !string.IsNullOrEmpty( token ) )
			{
				UserRepository userRepository = new UserRepository();
				UserModel inactiveUser = userRepository.GetByUsername( user );
				if ( inactiveUser != null && inactiveUser.ActivationCode == token )
				{
					try
					{
						userRepository.Activate( user );
					}
					catch ( Exception e )
					{
						return HttpNotFound( e.ToString() );
					}

					AuthenticateUser( user, false );
					return RedirectToAction( "Index", "Home" );
				}

				return HttpNotFound( string.Format( "User '{0}' not found or invalid token", user ) );
			}

			return HttpNotFound();
		}

		private void AuthenticateUser(string username, bool remember)
		{
			FormsAuthentication.SetAuthCookie( username, remember );
		}

		[Authorize]
		public ActionResult Edit()
		{
			UserRepository users = new UserRepository();
			UserModel user = users.GetByUsername( HttpContext.User.Identity.Name );

			UserSettingsModel settings = new UserSettingsModel
			{
				DateOfBirth = user.DateOfBirth,
				About = user.About,
				NotifyComment = user.NotifyComment,
				NotifyPhoto = user.NotifyPhoto,
				NotifySubscription = user.NotifySubscription
			};

			return View( settings );
		}

		[Authorize]
		[HttpPost]
		public ActionResult Edit(UserSettingsModel settings)
		{
			if ( ModelState.IsValid )
			{
				UserRepository users = new UserRepository();
				users.ModifySettings( HttpContext.User.Identity.Name, settings );

				return RedirectToAction( "Index", "Home" );
			}

			return View( settings );
		}

		[Authorize]
		public ActionResult ChangePassword()
		{
			return View();
		}

		[Authorize]
		[HttpPost]
		public ActionResult ChangePassword(ChangeExistingUserPasswordModel changePassword)
		{
			if ( ModelState.IsValid )
			{
				UserRepository users = new UserRepository();
				users.ChangePassword( HttpContext.User.Identity.Name, changePassword.NewPassword );

				return RedirectToAction( "Index", "Home" );
			}

			return View( changePassword );
		}

		public ActionResult ResetPassword(string user, string token)
		{
			ResetUserPasswordModel changePassword = new ResetUserPasswordModel();
			changePassword.Username = user;
			changePassword.Token = token;

			return View( changePassword );
		}

		[HttpPost]
		public ActionResult ResetPassword(ResetUserPasswordModel changePassword)
		{
			if ( ModelState.IsValid )
			{
				UserRepository users = new UserRepository();
				UserModel user = users.GetByUsername( changePassword.Username );

				if ( user != null && user.ActivationCode != null && user.ActivationCode == changePassword.Token )
				{
					users.ChangePassword( changePassword.Username, changePassword.NewPassword );
					users.Activate( changePassword.Username );

					return RedirectToAction( "Index", "Home" );
				}
				else
				{
					//ViewBag.ErrorMessage = "Invalid username/token pair.";
					return HttpNotFound( "Invalid username/token pair." );
				}
			}

			return View( changePassword );
		}

		public ActionResult RestorePassword()
		{
			return View();
		}

		[HttpPost]
		public ActionResult RestorePassword(RestoreUserPasswordModel restorePassword)
		{
			if ( ModelState.IsValid )
			{
				UserRepository users = new UserRepository();
				UserModel user = (users.GetByUsername( restorePassword.Account ) ?? users.GetByEmail( restorePassword.Account ));
				if ( user == null )
				{
					throw new Exception( string.Format( 
						"Username or e-mail address '{0}' not found - it should be already validated!", restorePassword.Account ) );
				}

				string activationCode = (new Random().Next( 1000 )).ToString().HashMD5();
				users.SetActivationCode( user.Login, activationCode );

				Uri requestUrl = Url.RequestContext.HttpContext.Request.Url;
				string activationLink = string.Format( "{0}://{1}{2}", requestUrl.Scheme, requestUrl.Authority,
					Url.Action( "ResetPassword", "User", new { user = Url.Encode( user.Login ), token = activationCode } ) );

				try
				{
					Helpers.SendEmail( user.Email, "PastExplorer password recovery",
						string.Format( "Hello, click <a href=\"{0}\">here</a> to change your PastExplorer account password.", activationLink ) );
				}
				catch ( Exception )
				{
					ViewBag.ErrorMessage = "Failed to send password recovery e-mail message. Please try again later.";
					return View( restorePassword );
				}

				return RedirectToAction( "Index", "Home" );
			}

			return View( restorePassword );
		}
	}
}
