using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhotoHistory.Models;
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
				else
				{
					return View( newUser );
				}
			}

			public ActionResult SignIn()
			{
				return View();
			}

    }
}
