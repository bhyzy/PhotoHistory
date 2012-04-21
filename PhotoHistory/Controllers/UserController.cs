using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhotoHistory.Model;
using PhotoHistory.Data;

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
			public ActionResult Create(string username, string password, string password2, string email)
			{
				UserRepository users = new UserRepository();
				Dictionary<string, string> errors = new Dictionary<string, string>();
				ViewBag.Errors = errors;

				// walidacja loginu
				if ( string.IsNullOrEmpty( username ) )
				{
					errors["username"] = "Username not specified";
				}
				else if ( username.Length > 100 )
				{
					errors["username"] = "Username is too long";
				}
				else if ( users.GetByUsername(username) != null )
				{
					errors["username"] = "Username already taken";
				}

				// walidacja hasla
				if ( string.IsNullOrEmpty( password ) )
				{
					errors["password"] = "Password not specified";
				}
				else if ( password.Length < 6 )
				{
					errors["password"] = "Password is too short";
				}
				else if ( password.Length > 100 )
				{
					errors["password"] = "Password is too long";
				}
				else
				{
					if ( string.IsNullOrEmpty( password2 ) )
					{
						errors["password2"] = "Password not confirmed";
					}
					else if ( password2 != password )
					{
						errors["password2"] = "Passwords don't match";
					}
				}

				// walidacja e-maila
				if ( string.IsNullOrEmpty( email ) )
				{
					errors["email"] = "E-mail address not specified";
				}
				else if ( email.Length > 100 )
				{
					errors["email"] = "E-mail address is too long";
				}
				else if (  )
				{
					errors["email"] = "Username already taken";
				}

				if ( errors.Count == 0 )
				{
					User user = new User()
					{
						Login = username,
						Password = password.HashMD5(),
						Email = email,
						Active = false,
						DateOfBirth = null,
						About = null,
						NotifyComment = true,
						NotifyPhoto = true,
						NotifySubscription = true
					};

					users.Create( user );
				}

				return View();
			}

			public ActionResult SignIn()
			{
				return View();
			}

    }
}
