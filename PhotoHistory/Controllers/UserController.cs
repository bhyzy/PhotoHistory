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
					Active = false,
					DateOfBirth = null,
					About = null,
					NotifyComment = true,
					NotifyPhoto = true,
					NotifySubscription = true
				};

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
			signUser.Remember = (form["Remember"] != null && form["Remember"].Equals( "on" ));

			if ( ModelState.IsValid )
			{
				UserRepository userRepository = new UserRepository();
				UserModel user = userRepository.GetByUsername( signUser.Login );
				if ( user != null && user.Password == signUser.Password.HashMD5() )
				{
					FormsAuthentication.SetAuthCookie( user.Login, signUser.Remember );

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
					ViewBag.InvalidCredentials = true;
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
	}
}
