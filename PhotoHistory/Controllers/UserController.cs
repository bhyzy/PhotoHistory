using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhotoHistory.Models;
using PhotoHistory.Data;
using System.Web.Security;

namespace PhotoHistory.Controllers
{
	public class UserController : Controller
	{
		//
		// GET: /User/

		public ActionResult Index()
		{
			return View();
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
			signUser.Remember = ( form["RememberMe"] != null );

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
						userRepository.Activate( inactiveUser );
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
	}
}
