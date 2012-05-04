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
            ViewBag.Albums = new UserRepository().GetByUsernameWithAlbums(HttpContext.User.Identity.Name).Albums;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddPhoto(NewPhotoModel photo, HttpPostedFileBase fileInput)
        {
            UserModel user = new UserRepository().GetByUsernameWithAlbums(HttpContext.User.Identity.Name);
            ViewBag.Albums = user.Albums;
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

                if (photo.Source == "remote")
                    fileInput = null;
                else
                    photo.PhotoURL = null;
                try
                {
                    string path=FileHelper.SaveRemoteOrLocal(fileInput, photo.PhotoURL, selectedAlbum);
                    System.Diagnostics.Debug.WriteLine("Photo uploaded successfully "+path);
                    if (string.IsNullOrEmpty(path))
                        throw new Exception("Can't save image");
                    PhotoRepository repo = new PhotoRepository();
                    PhotoModel newPhoto = new PhotoModel()
                    {
                        Path = path,
                        Date = DateTime.Parse(photo.Date),
                        Description = photo.Description,
                        Album = selectedAlbum
                    };
                    repo.Create(newPhoto);
                    System.Diagnostics.Debug.WriteLine("Created db entry " + newPhoto.Id);
                    return RedirectToAction("Show", new { id = photo.AlbumId });
                }
                catch (WrongPictureTypeException ex)
                {
                    ViewBag.ErrorMessage = "You must upload jpeg image.";
                    
                }
                catch (RemoteDownloadException ex) 
                {
                    ViewBag.ErrorMessage = "Can't upload your photo from provided URL. Please check your URL and try again later.";
                }
                catch (Exception ex) 
                {
                    ViewBag.ErrorMessage = "Can't upload your photo. Please try again later.";
                }
            }
            return View(photo);
        }

        public ActionResult ManageAlbum(int id)
        {
            AlbumRepository albums = new AlbumRepository();
            AlbumModel album = albums.GetById(id);
            return View(album);
        }


        [Authorize]
        public ActionResult Edit(int id)
        {
            AlbumRepository albums = new AlbumRepository();
            AlbumModel album = albums.GetById(id);
            PrepareCategories();
            return View(album);
        }


        [Authorize]
        [HttpPost]
        public ActionResult Edit(AlbumModel album)
        {
            //TODO access control
            if (ModelState.IsValid)
            {
                AlbumRepository albums = new AlbumRepository();
                AlbumModel dbAlbum = albums.GetById(album.Id);
                dbAlbum.Name = album.Name;
                dbAlbum.Description = album.Description;
                dbAlbum.Category = album.Category;
                dbAlbum.NotificationPeriod = album.NotificationPeriod; //NextNotification?
                dbAlbum.Public = album.Public;
                if (!dbAlbum.Public)
                {
                    //TODO move to model
                }
                dbAlbum.CommentsAllow = album.CommentsAllow;
                dbAlbum.CommentsAuth = album.CommentsAuth;

                albums.Update(dbAlbum);

                return RedirectToAction("Show", new { id = dbAlbum.Id });
            }
            PrepareCategories();
            return View(album);
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
            //TODO refactor
            if (Request["reminder"] == "remindYes")
            {
                System.DateTime today = System.DateTime.Now;
                try
                {
                    System.DateTime answer = today.AddDays(Int32.Parse(Request["NotificationPeriod"])); 
                    newAlbum.NextNotification = answer;
                }
                catch(Exception e){
                    ModelState.AddModelError("NotificationPeriod", "Number of days is incorrect");
                }                
            }
            // private access 
            //TODO refactor
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
