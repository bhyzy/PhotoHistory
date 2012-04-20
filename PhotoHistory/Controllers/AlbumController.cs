using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhotoHistory.Data;
using PhotoHistory.Model;

namespace PhotoHistory.Controllers
{
    public class AlbumController : Controller
    {
        //
        // GET: /Album/

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Browse()
		{
			UserRepository userRepo = new UserRepository();

			User user = new User()
			{
				Login = "kasia1337",
				Password = "tralala",
				Email = "kasia@buziaczek.pl"
			};
			userRepo.Create( user );

			User user2 = userRepo.GetById( user.Id );
			user2.Login = "Kasia666";
			userRepo.Update( user2 );

			userRepo.Delete( user2 );
			User user3 = userRepo.GetById( user2.Id );

			return View();
		}

		public ActionResult Charts()
		{
			return View();
		}

    }
}
