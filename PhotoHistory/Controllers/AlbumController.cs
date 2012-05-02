using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhotoHistory.Data;
using PhotoHistory.Models;
using NHibernate;
using System.Web.Security;

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

			UserModel user = new UserModel()
			{
				Login = "kasia1337",
				Password = "tralala",
				Email = "kasia@buziaczek.pl"
			};
			userRepo.Create( user );

			UserModel user2 = userRepo.GetById( user.Id );
			user2.Login = "Kasia666";
			userRepo.Update( user2 );

			userRepo.Delete( user2 );
			UserModel user3 = userRepo.GetById( user2.Id );

			return View();
		}

		public ActionResult Charts()
		{
			return View();
		}



        [Authorize]
        public ActionResult Create()
        {
            CategoryModel.EnsureStartingData();
            PrepareCategories();
            return View();
        }


        [Authorize]
        [HttpPost]
        public ActionResult Create(AlbumModel newAlbum, string a, string b)
        {
            PrepareCategories();

            //next notification 
            if (Request["reminder"] == "remindYes")
            {
                System.DateTime today = System.DateTime.Now;
                System.DateTime answer = today.AddDays(Int32.Parse(Request["NextNotificationDays"]));
                newAlbum.NextNotification = answer;
            }

            if (ModelState.IsValid)
            {                                
                //assign a current user
                UserRepository users = new UserRepository();
                newAlbum.User = 
                    users.GetByUsername(HttpContext.User.Identity.Name);
                
                // password
                if (newAlbum.Password != null)
                    newAlbum.Password = newAlbum.Password.HashMD5();

                AlbumRepository albums = new AlbumRepository();
                albums.Create(newAlbum);

                return View("Created", newAlbum);
            }


            return View(newAlbum);
        }


        // loads categories as a list
        private void PrepareCategories()
        {
            using (var session = SessionProvider.SessionFactory.OpenSession())
            {
                ViewData["ListOfCategories"] = 
                    new SelectList(session.QueryOver<CategoryModel>().List(), "Id", "Name");
            }
        }

    }
}
