using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhotoHistory.Data;
using PhotoHistory.Models;
using NHibernate;
using System.Web.Security;
using PhotoHistory.Common;

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


        public ActionResult Show(int id)
        {
            AlbumRepository albums = new AlbumRepository();
            AlbumModel album = albums.GetById(id);
            return View(album);
        }

        [Authorize]
        public ActionResult Manage()
        {
            UserRepository users = new UserRepository();
            UserModel user = users.GetByUsernameWithAlbums(HttpContext.User.Identity.Name);
            return View(user.Albums);
        }

        [Authorize]
        public ActionResult AddPhoto()
        {
            System.Diagnostics.Debug.WriteLine("TUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU");
            ViewBag.Albums = new UserRepository().GetByUsernameWithAlbums(HttpContext.User.Identity.Name).Albums;
               
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddPhoto(NewPhotoModel photo, HttpPostedFileBase fileInput)
        {
            System.Diagnostics.Debug.WriteLine("HERE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            UserModel user = new UserRepository().GetByUsernameWithAlbums(HttpContext.User.Identity.Name);
            ViewBag.Albums = user.Albums;
            if(photo.Source=="remote")
                System.Diagnostics.Debug.WriteLine("HERE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            if (ModelState.IsValid)
            {   
                if (photo.PhotoURL == null && fileInput == null)
                {
                    ViewBag.ErrorMessage = "You must select file for upload.";
                    return View(photo);
                }
                AlbumModel selectedAlbum = null;
                foreach (AlbumModel album in user.Albums)
                {
                    if (album.Id == photo.AlbumId)
                    {
                        selectedAlbum = album;
                        break;
                    }
                }

                if (fileInput != null)
                    if (fileInput.ContentType == "image/jpeg")
                    {
                        //handle jpg upload
                        FileHelper.savePhoto(fileInput.InputStream, selectedAlbum);
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Only jpeg files are allowed";
                        return View(photo);
                    }

            }
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
        public ActionResult Create(AlbumModel newAlbum)
        {
            PrepareCategories();

            //next notification 
            if (Request["reminder"] == "remindYes")
            {
                System.DateTime today = System.DateTime.Now;
                try
                {
                    System.DateTime answer = today.AddDays(Int32.Parse(Request["NextNotificationDays"])); 
                    newAlbum.NextNotification = answer;
                }
                catch(Exception e){
                    ModelState.AddModelError("NextNotification", "Number of days is incorrect");
                }                
            }
            // private access
            UserModel[] userModels = null; //an array of trusted users
            if (!newAlbum.Public)
            {
                switch (Request["privateMode"])
                {
                    case "password":
                        if (newAlbum.Password != null)
                            newAlbum.Password = newAlbum.Password.HashMD5();
                        else
                            ModelState.AddModelError("Password", "You didn't provide a password");
                        break;
                    case "users":
                        if (Request["usersList"] == null)
                        {
                            ModelState.AddModelError("Users", "You didn't provide a user list");
                            break;
                        }
                        string[] userLogins = Request["usersList"].Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                        userModels = AlbumModel.FindUsersByLogins(userLogins);
                        if (userModels == null)
                            ModelState.AddModelError("Users", "At least one login you provided is incorrect.");
                        break;
                }
            }
            if (ModelState.IsValid)
            {                                
                //assign a current user
                UserRepository users = new UserRepository();
                newAlbum.User = 
                    users.GetByUsername(HttpContext.User.Identity.Name);

                AlbumRepository albums = new AlbumRepository();
                albums.Create(newAlbum);

                // if there are trusted users, add them
                if (userModels != null)
                    foreach (var user in userModels)
                        newAlbum.CreateTrustedUser(user);

                return RedirectToAction("Show", new { id = newAlbum.Id } );
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
