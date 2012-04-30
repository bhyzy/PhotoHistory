using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhotoHistory.Data;
using PhotoHistory.Models;
using NHibernate;

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

        
        
        
        public ActionResult Create()
        {
            CategoryModel.EnsureStartingData();            
            return View();
        }

        
        [HttpPost]
        public ActionResult Create(AlbumModel newAlbum)
        {
            if (ModelState.IsValid)
            {
                AlbumRepository albums = new AlbumRepository();                
                albums.Create(newAlbum);

                return View("Created", newAlbum);
            }

            return View(newAlbum);
        }


    }
}
