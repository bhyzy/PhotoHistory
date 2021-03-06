﻿using System;
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
using System.Diagnostics;
using System.Drawing.Imaging;

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

        public ActionResult Browse(int? catId, int? pageNbr)
        {
            int maxItemsPerPage = 6;
            int nbr = pageNbr ?? 1;
            if (nbr < 1)
                nbr = 1;
            @ViewBag.Category = MainCategory.Browse;
            BrowseAlbumModel model = new BrowseAlbumModel();
            CategoryRepository catRepo = new CategoryRepository();
            model.Categories = catRepo.getCategories();
            model.SelectedCategory = model.Categories.First().Id ?? 0;


            foreach (CategoryModel category in model.Categories)
            {
                if (category.Id == catId)
                {
                    model.SelectedCategory = category.Id ?? 0;
                    break;
                }
            }

            AlbumRepository albumRepo = new AlbumRepository();
            AlbumModel[] albums = albumRepo.GetByCategory(catRepo.GetById(catId ?? model.Categories.First().Id), true, true, true, true, false).ToArray();
            model.PageCount = (int)Math.Ceiling(albums.Length / (double)maxItemsPerPage);
            if (model.PageCount < 1)
                model.PageCount = 1;
            if (nbr > model.PageCount)
                nbr = model.PageCount;
            int start = (nbr - 1) * maxItemsPerPage;
            int end = nbr * maxItemsPerPage;
            model.CurrentPage = nbr;

            end = (end >= albums.Length) ? albums.Length : end;
            model.Albums = new List<AlbumProfileModel>();
            string startS, endS;
            for (int i = start; i < end; ++i)
            {
                AlbumModel al = albums[i];
                Helpers.AlbumDateRange(al, out startS, out endS);

                AlbumProfileModel album = new AlbumProfileModel
                {
                    Comments = al.Comments.Count,
                    Id = al.Id,
                    Name = al.Name,
                    Rating = al.Rating,
                    Views = al.Views,
                    StartDate = startS,
                    EndDate = endS,
                    Thumbnails = Helpers.AlbumThumbnails(al)
                };
                model.Albums.Add(album);
            }
            model.Albums.Reverse();
            return View(model);
        }

        public ActionResult Charts(ChartCategory? category)
        {
            const int maxAlbums = 4;

            HomepageAlbumModel model = null;
            AlbumRepository repo = new AlbumRepository();
            @ViewBag.Category = MainCategory.Charts;
            category = category ?? ChartCategory.Popular;
            if (!Enum.IsDefined(typeof(ChartCategory), category))
                category = ChartCategory.Popular;
            @ViewBag.Chart = category;
            switch (category)
            {
                case ChartCategory.Popular: { model = new HomepageAlbumModel() { Name = "Most popular", Albums = Helpers.Convert(repo.GetMostPopular(maxAlbums)) }; break; }
                case ChartCategory.Biggest: { model = new HomepageAlbumModel() { Name = "Biggest", Albums = Helpers.Convert(repo.GetBiggest(maxAlbums)) }; break; }
                case ChartCategory.MostComments: { model = new HomepageAlbumModel() { Name = "Most commented", Albums = Helpers.Convert(repo.GetMostCommented(maxAlbums)) }; break; }
                case ChartCategory.TopRated: { model = new HomepageAlbumModel() { Name = "Highest rated", Albums = Helpers.Convert(repo.GetTopRated(maxAlbums)) }; break; }
            }
            return View(model);
        }

        [Authorize]
        public ActionResult Show(int id)
        {
            AlbumRepository albums = new AlbumRepository();
            AlbumModel album = albums.GetByIdForShow(id);

            UserRepository users = new UserRepository();
            var user = users.GetByUsername(HttpContext.User.Identity.Name);

            // check if album has a password, if it does, authorize
            if (!albums.authorizeWithPassword(album, user, (string)Session["Album" + album.Id.ToString()]))
                return RedirectToAction("PasswordForAlbum", new { id = album.Id });

            // if user is not authorized
            if (!albums.IsUserAuthorizedToViewAlbum(album, user, true))
                return View("NotAuthorized");

            if (user == null || user.Id != album.User.Id) //if not logged in or not an author
            {
                //increment views
                album.Views += 1;
                albums.Update(album);
            }
            @ViewBag.user = user;
            return View(album);
        }


        [Authorize]
        public ActionResult PasswordForAlbum(int id)
        {
            AlbumRepository albums = new AlbumRepository();
            AlbumModel album = albums.GetById(id);
            return View(album);
        }

        /// <summary>
        /// receives password from a form, saves it in session and grants (or not) access to album
        /// </summary>
        /// <param name="password">password from form</param>
        /// <param name="id">id of an album</param>
        /// <returns>redirect to show album if access granted, else show error</returns>
        [Authorize]
        [HttpPost]
        public ActionResult PasswordForAlbum(int id, string password)
        {
            AlbumRepository albums = new AlbumRepository();
            AlbumModel album = albums.GetById(id);
            if (password.HashMD5() == album.Password)
            {
                Session["Album" + album.Id.ToString()] = password.HashMD5();
                return RedirectToAction("Show", new { id = album.Id });
            }
            else
            {
                ViewBag.MyErrorMsg = "Wrong password";
                return View(album);
            }
        }


        [Authorize]
        public ActionResult Manage()
        {
            UserRepository users = new UserRepository();
            UserModel user = users.GetByUsernameWithAlbums(HttpContext.User.Identity.Name, false, false, true, true);
            ViewBag.Action = new string[] { "Album", "ManageAlbum" };
            return View(Helpers.Convert(user.Albums.ToList()));
        }

        [Authorize]
        public ActionResult AddPhoto(int? albumId)
        {
            ViewBag.Albums = new UserRepository().GetByUsernameWithAlbums(HttpContext.User.Identity.Name).Albums;
            ViewBag.selected = albumId;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddPhoto(NewPhotoModel photo)
        {
            UserModel user = new UserRepository().GetByUsernameWithAlbums(HttpContext.User.Identity.Name, withFollowers: true);
            ViewBag.Albums = user.Albums;

            if (photo.Source == "remote")
                photo.FileInput = null;
            else
                photo.PhotoURL = null;
            ITransaction transaction = null;
            ISession session = null;
            try
            {
                using (Image img = FileHelper.PrepareImageFromRemoteOrLocal(photo))
                {
                    if (img == null)
                        throw new FileUploadException("Can't upload your photo. Please try again later.");

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

                        session = SessionProvider.SessionFactory.OpenSession();
                        transaction = session.BeginTransaction();

                        string photoName = "photo_" + DateTime.Now.ToString("yyyyMMddHHmmssff");
                        string path = FileHelper.getPhotoPathWithoutExtension(selectedAlbum, photoName) + ".jpg";
                        if (string.IsNullOrEmpty(path))
                            throw new Exception("Can't save image");

                        path = FileHelper.SavePhoto(img, selectedAlbum, photoName);
                        if (string.IsNullOrEmpty(path))
                            throw new Exception("Returned path is empty");

                        PhotoRepository repo = new PhotoRepository();
                        PhotoModel newPhoto = new PhotoModel()
                        {
                            Path = path,
                            Date = DateTime.Parse(photo.Date),
                            Description = photo.Description,
                            Album = selectedAlbum
                        };

                        double? locLatitude = img.GPSLatitude();
                        double? locLongitude = img.GPSLongitude();
                        if (locLatitude.HasValue && locLongitude.HasValue)
                        {
                            newPhoto.LocationLatitude = locLatitude.Value;
                            newPhoto.LocationLongitude = locLongitude.Value;
                        }

                        repo.Create(newPhoto);
                        System.Diagnostics.Debug.WriteLine("Created db entry " + newPhoto.Id);

                        transaction.Commit();
                        Helpers.NotifyAlbumObservers(newPhoto.Album);
                        return RedirectToAction("Show", new { id = photo.AlbumId });

                    }
                }
            }
            catch (FileUploadException ex)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                }
                if (session != null)
                    session.Dispose();
                ModelState.AddModelError("FileInput", ex.Message);
            }
            catch (Exception)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                }
                if (session != null)
                    session.Dispose();
                ModelState.AddModelError("FileInput", "Can't upload your photo. Please try again later.");
            }
            return View(photo);
        }

        [Authorize]
        public ActionResult ManageAlbum(int id)
        {
            AlbumRepository albums = new AlbumRepository();
            AlbumModel album = albums.GetByIdForManage(id);
            UserRepository users = new UserRepository();
            var user = users.GetByUsername(HttpContext.User.Identity.Name);

            //access control
            if (!albums.isUserAuthorizedToEditAlbum(album, user))
                return View("NotAuthorizedEdit");

            return View(album);
        }


        [Authorize]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            //TODO access control
            AlbumRepository albums = new AlbumRepository();
            AlbumModel album = albums.GetById(id);
            albums.Delete(album);
            return RedirectToAction("Manage");
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeletePhotos(int albumId, int[] selectedObjects)
        {
            //TODO access control
            PhotoRepository photos = new PhotoRepository();
            if (selectedObjects != null)
            {
                foreach (int id in selectedObjects)
                {
                    PhotoModel photo = photos.GetById(id);
                    if (photo.Album.Id == albumId)
                        photos.Delete(photo);
                }
            }
            return RedirectToAction("ManageAlbum", new { id = albumId });
        }


        [Authorize]
        public ActionResult Edit(int id)
        {
            AlbumRepository albums = new AlbumRepository();
            AlbumModel album = albums.GetByIdForEdit(id);

            UserRepository users = new UserRepository();
            var user = users.GetByUsername(HttpContext.User.Identity.Name);

            //access control
            if (!albums.isUserAuthorizedToEditAlbum(album, user))
                return View("NotAuthorizedEdit");


            PrepareCategories();
            ViewData["usersList"] = string.Join(", ", album.TrustedUsers.Select(u => u.Login));
            return View(album);
        }


        [Authorize]
        [HttpPost]
        public ActionResult Edit(AlbumModel album)
        {
            SetNextNotification(album);

            SetPrivateAccess(album);

            if (ModelState.IsValid)
            {
                AlbumRepository albums = new AlbumRepository();
                AlbumModel dbAlbum = albums.GetByIdForEdit(album.Id);
                dbAlbum.Name = album.Name;
                dbAlbum.Description = album.Description;
                dbAlbum.Category = album.Category;
                dbAlbum.Public = album.Public;
                dbAlbum.Password = album.Password;
                dbAlbum.CommentsAllow = album.CommentsAllow;
                dbAlbum.CommentsAuth = album.CommentsAuth;
                dbAlbum.TrustedUsers = album.TrustedUsers;
                dbAlbum.NotificationPeriod = album.NotificationPeriod;
                dbAlbum.NextNotification = album.NextNotification;
                albums.Update(dbAlbum);

                return RedirectToAction("Show", new { id = dbAlbum.Id });
            }
            PrepareCategories();
            return View(album);
        }


        [Authorize]
        public ActionResult Create()
        {
            PrepareCategories();
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Create(AlbumModel newAlbum)
        {
            PrepareCategories();

            SetNextNotification(newAlbum);

            // private access 
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


        // AJAX: /Album/Subscribe
        [HttpPost]
        public ActionResult Subscribe(int albumId, bool unsubscribe)
        {
            AlbumRepository albums = new AlbumRepository();
            AlbumModel album = albums.GetById(albumId, withFollowers: true);
            UserRepository users = new UserRepository();
            UserModel user = users.GetByUsername(HttpContext.User.Identity.Name);
            string[] response = new string[2];
            // reponse[0] is an operation code
            // response[1] is a message for a user
            if (user != null)
            {
                //subscribe if the user is logged in
                albums.Subscribe(album, user, unsubscribe);
                if (unsubscribe)
                {
                    response[0] = "unsubscribed";
                    response[1] = "You stopped following this album";
                }
                else
                {
                    response[0] = "subscribed";
                    response[1] = "You are now following this album";
                }
            }
            else
            {
                response[0] = "error"; //error
                response[1] = "You need to be logged in to vote";
            }
            return Json(response);
        }

        // AJAX: /Album/Vote/5
        [HttpPost]
        public ActionResult Vote(int id, bool up)
        {
            AlbumRepository albums = new AlbumRepository();
            AlbumModel album = albums.GetById(id);
            UserRepository users = new UserRepository();
            UserModel user = users.GetByUsername(HttpContext.User.Identity.Name);
            string[] response = new string[2];
            // reponse[0] is a new rating
            // response[1] is a message for a user
            if (user != null)
            {
                //create vote if the user is logged in
                if (album.CreateVote(user, up))
                    response[1] = "Your vote has been saved.";
                else
                    response[1] = "You have already voted on this album !";
            }
            else
            {
                response[1] = "You need to be logged in to vote";
            }
            response[0] = album.getRating().ToString();
            return Json(response);
        }

        // AJAX: /Album/AcceptComment
        [HttpPost]
        public ActionResult AcceptComment(int id)
        {
            AlbumRepository albums = new AlbumRepository();
            CommentModel comment = albums.GetComment(id, true);
            UserRepository users = new UserRepository();
            UserModel user = users.GetByUsername(HttpContext.User.Identity.Name);
            string[] response = new string[2];

            if (user != null)
            {
                if (comment.Album.User.Id == user.Id) //ok
                {
                    response[0] = "ok";
                    response[1] = "";
                    comment.Accepted = true;
                    albums.Update(comment);
                }
                else
                {
                    response[0] = "error";
                    response[1] = "You are not allowed to accept this comment.";
                }
            }
            else
            {
                response[0] = "error";
                response[1] = "You need to be logged in to accept comments";
            }

            return Json(response);
        }


        // AJAX: /Album/DeleteComment
        [HttpPost]
        public ActionResult DeleteComment(int id)
        {
            AlbumRepository albums = new AlbumRepository();
            CommentModel comment = albums.GetComment(id, true, true);
            UserRepository users = new UserRepository();
            UserModel user = users.GetByUsername(HttpContext.User.Identity.Name);
            string[] response = new string[2];

            if (user != null)
            {
                if (comment.User.Id == user.Id || comment.Album.User.Id == user.Id) //usuwac moze wlasciciel albumu lub komentarza
                {
                    response[0] = "ok";
                    response[1] = "";
                    albums.deleteComment(comment);
                }
                else
                {
                    response[0] = "error";
                    response[1] = "You are not allowed to delete this comment.";
                }
            }
            else
            {
                response[0] = "error";
                response[1] = "You need to be logged in to delete comments";
            }

            return Json(response);
        }

        // AJAX: /Album/Comment
        [HttpPost]
        public ActionResult Comment(int id, String comment)
        {
            AlbumRepository albums = new AlbumRepository();
            AlbumModel album = albums.GetById(id, withUser: true);
            UserRepository users = new UserRepository();
            UserModel user = users.GetByUsername(HttpContext.User.Identity.Name);

            NewCommentModel newComment = new NewCommentModel();
            //Wylaczone komentowanie
            if (!album.CommentsAllow)
            {
                newComment.Message = "You can't comment this album";
                return Json(newComment);
            }

            CommentModel model = new CommentModel();
            model.Album = album;
            model.Body = comment;
            model.Date = DateTime.Now;
            model.User = user;

            //Komentarze wlasciciela albumu sa automatycznie akceptowane, komentarze innego uzytkownika sa akceptowane 
            //jesli wylaczono opcje autoryzacji
            newComment.Accepted = (album.User.Id == user.Id) || !album.CommentsAuth;
            newComment.Body = comment;
            newComment.Date = model.Date.ToString("dd/MM/yyyy HH:mm:ss");
            newComment.UserName = user.Login;
            newComment.Link = @Url.Action("ViewProfile", "User", new { userName = model.User.Login });

            model.Accepted = newComment.Accepted;
            if (user != null)
            {
                if (albums.AddComment(model))
                {
                    newComment.Id = model.Id ?? 1;
                    newComment.Message = "Your comment has been saved.";
                    //funkcja automatycznie sprawdza czy wyslac powiadomienie
                    Helpers.NotifyCommentObserver(model);
                }
                else
                {
                    newComment.Message = "Can't add comment.";
                    newComment.Body = null;
                }

            }
            else
            {
                newComment.Body = null;
                newComment.Message = "You need to be logged in to comment";
            }

            return Json(newComment);
        }



        // ------------ PRIVATE METHODS ------------------

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
            if (album.Public)
            {
                // make sure we dont read anything from 'password' field
                album.Password = null;
            }
            else
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
                    default: // album visible only to owner
                        album.Password = null; //nullify password, because its checkbox was not ticked
                        break;
                }
            return userModels;
        }

        private void SetNextNotification(AlbumModel album)
        {
            //next notification 
            if (Request["reminder"] == "remindYes")
            {
                System.DateTime today = System.DateTime.Now;
                try
                {
                    int tmp = Int32.Parse(Request["NotificationPeriod"]);
                    System.DateTime answer = today.AddDays(tmp);
                    album.NextNotification = answer;
                }
                catch (Exception)
                {
                    ModelState.AddModelError("NotificationPeriod", "Number of days is incorrect");
                }
            }
            else
            {
                album.NextNotification = null;
                album.NotificationPeriod = null;
                ModelState.Clear(); //clear errors
                TryValidateModel(album); //revalidate
            }
        }

    }
}
