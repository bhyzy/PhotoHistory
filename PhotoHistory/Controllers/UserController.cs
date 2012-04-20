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
				if ( username.Length > 0 && password.Length > 0 && password2 == password && email.Length > 0 )
				{
					User newUser = new User()
					{
						Login = username,
						Password = password,
						Email = email,
						Active = false,
						DateOfBirth = null,
						About = null,
						NotifyComment = true,
						NotifyPhoto = true,
						NotifySubscription = true
					};

					UserRepository userRepo = new UserRepository();
					userRepo.Create( newUser );

					return Content( "User account created ;)" );
				}

				return View();
			}

			public ActionResult SignIn()
			{
				return View();
			}

    }
}
