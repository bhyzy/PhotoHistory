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
using System.Drawing;

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
            userRepo.Create(user);

            UserModel user2 = userRepo.GetById(user.Id);
            user2.Login = "Kasia666";
            userRepo.Update(user2);

            userRepo.Delete(user2);
            UserModel user3 = userRepo.GetById(user2.Id);

            return View();
        }

        public ActionResult Charts()
        {
            return View();
        }


        public ActionResult Show(int id)
        {
            AlbumRepository albums = new AlbumRepository();
            AlbumModel album = albums.GetByIdWithUser(id);
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
        public ActionResult AddPhoto(NewPhotoModel photo)
        {
            UserModel user = new UserRepository().GetByUsernameWithAlbums(HttpContext.User.Identity.Name);
            ViewBag.Albums = user.Albums;

            Image img = null;

            if (photo.Source == "remote")
                photo.FileInput = null;
            else
                photo.PhotoURL = null;

            try
            {
                img = FileHelper.PrepareImageFromRemoteOrLocal(photo);
                if (img == null)
                    throw new FileUploadException("Can't upload your photo. Please try again later.");
            }
            catch (FileUploadException ex)
            {
                ModelState.AddModelError("FileInput", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("FileInput", "Your file is invalid.");
            }

            if (ModelState.IsValid)
            {
                AlbumModel selectedAlbum = null;
                foreach (AlbumModel album in user.Albums)
                {
                    if (album.Id == photo.AlbumId)
                    {
                        selectedAlbum = album;
                        break;
                    }
                }

                using(ISession session= SessionProvider.SessionFactory.OpenSession())
                using(ITransaction transaction = session.BeginTransaction())
                {
                    try
                    {
                        string photoName = "photo_" + DateTime.Now.ToString("yyyyMMddHHmmssff");
                        string path = FileHelper.getPhotoPath(selectedAlbum, photoName); 
                    
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
                        path = FileHelper.SavePhoto(img, selectedAlbum, photoName);
                        if(string.IsNullOrEmpty(path))
                            throw new Exception("Returned path is empty");
                        transaction.Commit();
                        return RedirectToAction("Show", new { id = photo.AlbumId });
                    }
                    catch (FileUploadException ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("FileInput", ex.Message);
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("FileInput","Can't upload your photo. Please try again later.");
                    }
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
            AlbumModel album = albums.GetByIdForEdit(id);
            PrepareCategories();
            return View(album);
        }


        [Authorize]
        [HttpPost]
        public ActionResult Edit(AlbumModel album)
        {
            //TODO access control
                      
            if (!album.Public)
                SetPrivateAccess(album);

            if (ModelState.IsValid)
            {
                AlbumRepository albums = new AlbumRepository();
                AlbumModel dbAlbum = albums.GetByIdForEdit(album.Id);
                dbAlbum.Name = album.Name;
                dbAlbum.Description = album.Description;
                dbAlbum.Category = album.Category;
                dbAlbum.NotificationPeriod = album.NotificationPeriod; //NextNotification?
                dbAlbum.Public = album.Public;
                dbAlbum.Password = album.Password;
                dbAlbum.CommentsAllow = album.CommentsAllow;
                dbAlbum.CommentsAuth = album.CommentsAuth;
                dbAlbum.TrustedUsers = album.TrustedUsers;
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
            if (Request["reminder"] == "remindYes")
            {
                System.DateTime today = System.DateTime.Now;
                try
                {
                    System.DateTime answer = today.AddDays(Int32.Parse(Request["NotificationPeriod"]));
                    newAlbum.NextNotification = answer;
                }
                catch (Exception)
                {
                    ModelState.AddModelError("NotificationPeriod", "Number of days is incorrect");
                }
            }
            // private access 
            if (!newAlbum.Public)
                SetPrivateAccess(newAlbum);

            if (ModelState.IsValid)
            {
                //assign a current user
                UserRepository users = new UserRepository();
                newAlbum.User =
                    users.GetByUsername(HttpContext.User.Identity.Name);

                AlbumRepository albums = new AlbumRepository();
                albums.Create(newAlbum);

                return RedirectToAction("Show", new { id = newAlbum.Id });
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


        // handles private access settings from form
        private UserModel[] SetPrivateAccess(AlbumModel album)
        {
            UserModel[] userModels = null; //an array of trusted users
            switch (Request["privateMode"])
            {
                case "password":
                    if (album.Password != null)
                        album.Password = album.Password.HashMD5();
                    else
                        ModelState.AddModelError("Password", "You didn't provide a password");
                    break;
                case "users":
                    album.Password = null; //nullify password, because its checkbox was not ticked
                    if (string.IsNullOrEmpty(Request["usersList"]))
                    {
                        ModelState.AddModelError("Users", "You didn't provide a user list");
                        break;
                    }
                    string[] userLogins = Request["usersList"].Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    userModels = AlbumModel.FindUsersByLogins(userLogins);
                    if (userModels == null)
                        ModelState.AddModelError("Users", "At least one login you provided is incorrect.");
                    else
                        album.TrustedUsers = userModels;
                    break;
                default:
                    album.Password = null; //nullify password, because its checkbox was not ticked
                    break;
            }
            return userModels;
        }

    }
}
