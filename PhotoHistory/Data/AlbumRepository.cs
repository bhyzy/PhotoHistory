﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PhotoHistory.Models;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace PhotoHistory.Data
{
    public class AlbumRepository : DataRepository<AlbumModel, Int32?>
    {
        public override void Create(AlbumModel obj)
        {
            using (var session = GetSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    session.Save(obj);
                    transaction.Commit();
                }
            }
        }

        public void Subscribe(AlbumModel album, UserModel user, bool unsubscribe = false)
        {
            try
            {
                using (var session = GetSession())
                {
                    using (var transaction = session.BeginTransaction())
                    {
                        if (unsubscribe)
                        {
                            UserModel found = album.Followers.Single(delegate(UserModel model)
                            {
                                return model.Id == user.Id;
                            });
                            if (found != null)
                            {
                                album.Followers.Remove(found);
                            }
                        }
                        else
                        {
                            album.Followers.Add(user);
                        }
                        session.Update(album);
                        transaction.Commit();
                    }
                }
            }
            catch (Exception) { }
        }

        public CommentModel GetComment(int id, bool withAlbum = false, bool withUser=false)
        {
            using (var session = GetSession())
            {
                CommentModel comment = session.CreateQuery("from CommentModel where Id = :id").SetParameter("id",id).UniqueResult<CommentModel>();
                if (comment != null)
                {
                    if(withAlbum)
                        comment.Album.User.ToString();
                    if (withUser)
                        comment.User.ToString();
                }
                    
                return comment;
            }
        }

        public void deleteComment(CommentModel obj)
        {
            using (var session = GetSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    session.Delete(obj);
                    transaction.Commit();
                }
            }
        }

        public bool AddComment(CommentModel comment)
        {
            if (string.IsNullOrEmpty(comment.Body))
                return false;
            try
            {
                using (var session = GetSession())
                {
                    using (var transaction = session.BeginTransaction())
                    {
                        session.Save(comment);
                        transaction.Commit();
                        return true;
                    }
                }
            }
            catch (Exception )
            {
                return false;
            }
        }

        public List<AlbumModel> GetAll()
        {
            using (var session = GetSession())
            {
                List<AlbumModel> albums= session.CreateQuery("from AlbumModel").Enumerable<AlbumModel>().ToList<AlbumModel>();
                foreach (AlbumModel album in albums)
                    album.User.ToString();
                return albums;
            }

        }

        public List<AlbumModel> GetByCategory(CategoryModel category, bool withUser = false, bool withPhotos = false, bool withComments = false,
              bool withCategory = false, bool withTrustedUsers = false, bool withFollowers = false)
        {
            using (var session = GetSession())
            {
                List<AlbumModel> albums = session.CreateQuery("from AlbumModel where Category = :cat").SetParameter("cat", category).Enumerable<AlbumModel>().ToList<AlbumModel>();
                if (albums != null)
                {
                    foreach (AlbumModel album in albums)
                    {
                        if (withComments) album.Comments.ToList().Sort(delegate(CommentModel a, CommentModel b)
                        {
                            return a.Date.CompareTo(b.Date);
                        });
                        if (withUser) album.User.ToString();
                        if (withPhotos) album.Photos.ToString();
                        if (withCategory) album.Category.ToString();
                        if (withTrustedUsers) album.TrustedUsers.ToList();
                        if (withFollowers) album.Followers.ToList();
                    }
                }
                return albums;
            }
        }

        public override AlbumModel GetById(int? id)
        {
            using (var session = GetSession())
            {
                return session.CreateQuery("from AlbumModel where Id = :id").SetParameter("id", id).UniqueResult<AlbumModel>();
            }
        }

        public AlbumModel GetById(int? id,
            bool withUser = false, bool withPhotos = false, bool withComments = false,
            bool withCategory = false, bool withTrustedUsers = false, bool withFollowers = false)
        {
            using (var session = GetSession())
            {
                AlbumModel album = session.CreateQuery("from AlbumModel where Id = :id").SetParameter("id", id).UniqueResult<AlbumModel>();
                if (album != null)
                {
                    if (withComments) album.Comments.ToList().Sort(delegate(CommentModel a, CommentModel b)
                        {
                            return a.Date.CompareTo(b.Date);
                        });
                    foreach (CommentModel comment in album.Comments)
                        comment.User.ToString();
                    if (withUser) album.User.ToString();
                    if (withPhotos) album.Photos.ToString();
                    if (withCategory) album.Category.ToString();
                    if (withTrustedUsers) album.TrustedUsers.ToList();
                    if (withFollowers) album.Followers.ToList();
                }
                return album;
            }
        }

        public AlbumModel GetWithComments(int? id)
        {
            return GetById(id, withComments: true);
        }

        public AlbumModel GetByIdWithPhotos(int? id, bool with)
        {
            return GetById(id, withPhotos: true);
        }

        public AlbumModel GetByIdWithUser(int? id)
        {
            return GetById(id, withUser: true);
        }

        public List<AlbumModel> GetMostPopular(int maxAlbums)
        {
            List<AlbumModel> models = new List<AlbumModel>();
            using (var session = GetSession())
            {
                var criteria = session.CreateCriteria<AlbumModel>().SetMaxResults(maxAlbums).AddOrder(Order.Desc("Views"));
                models = criteria.List<AlbumModel>().ToList();
                foreach (AlbumModel album in models)
                {
                    album.Photos.ToList();
                    album.Comments.ToList();
                }

            }
            return models;
        }

        public List<AlbumModel> GetTopRated(int maxAlbums)
        {
            List<AlbumModel> models = new List<AlbumModel>();
            using (var session = GetSession())
            {
                var criteria = session.CreateCriteria<AlbumModel>().SetMaxResults(maxAlbums).AddOrder(Order.Desc("Rating"));
                models = criteria.List<AlbumModel>().ToList();
                foreach (AlbumModel album in models)
                {
                    album.Photos.ToList();
                    album.Comments.ToList();
                }
            }
            return models;
        }

        public List<AlbumModel> GetBiggest(int maxAlbums)
        {
            List<AlbumModel> models = new List<AlbumModel>();
            using (var session = GetSession())
            {
                var sql = session.CreateSQLQuery("select album_id,count(album_id) albums from Albums join Photos using(album_id) group by album_id order by albums desc;");
                List<object[]> list = sql.List<object[]>().Take<object[]>(maxAlbums).ToList<object[]>();
                AlbumModel album;
                foreach (object[] item in list)
                {
                    album = session.CreateQuery("from AlbumModel where Id = :id").SetParameter("id", item[0]).UniqueResult<AlbumModel>();
                    album.Photos.ToList();
                    album.Comments.ToList();
                    models.Add(album);
                }
            }
            return models;
        }

        public List<AlbumModel> GetRandom(int maxAlbums)
        {
            List<AlbumModel> models = new List<AlbumModel>();
            using (var session = GetSession())
            {
                var query = session.CreateQuery("from AlbumModel order by RANDOM()").SetMaxResults(maxAlbums);
                models = query.List<AlbumModel>().ToList();
                foreach (AlbumModel album in models)
                {
                    album.Photos.ToList();
                    album.Comments.ToList();
                }
            }
            return models;
        }

        public List<AlbumModel> GetRecentlyCommented(int maxAlbums)
        {
            List<AlbumModel> models = new List<AlbumModel>();
            using (var session = GetSession())
            {
                var albumQuery = session.CreateSQLQuery(String.Format("select distinct album_id from (select album_id,date_posted  from Comments c order by c.date_posted ) a limit {0};", maxAlbums));
                var albumsId = albumQuery.List<int>();

                AlbumModel album;
                foreach (int id in albumsId)
                {
                    album = session.CreateQuery("from AlbumModel where Id = :id").SetParameter("id", id).UniqueResult<AlbumModel>();
                    album.Photos.ToList();
                    album.Comments.ToList();
                    models.Add(album);
                }
                return models;
            }
        }

        public List<AlbumModel> GetMostCommented(int maxAlbums)
        {
            List<AlbumModel> models = new List<AlbumModel>();

            using (var session = GetSession())
            {
                var albumQuery = session.CreateSQLQuery(String.Format("select album_id, count(album_id) as albCount from Comments c group by album_id order by albCount desc limit {0};", maxAlbums));
                var albumsId = albumQuery.List<object[]>();

                AlbumModel album = GetWithComments(1);
                foreach (object[] item in albumsId)
                {
                    album = session.CreateQuery("from AlbumModel where Id = :id").SetParameter("id", item[0]).UniqueResult<AlbumModel>();
                    album.Photos.ToList<PhotoModel>();
                    album.Comments.ToList<CommentModel>();
                    models.Add(album);
                }
                return models;
            }
        }

        public AlbumModel GetByIdForEdit(int? id)
        {
            using (var session = GetSession())
            {
                var album = session.CreateQuery("from AlbumModel where Id = :id").SetParameter("id", id).UniqueResult<AlbumModel>();
                album.TrustedUsers.ToList();
                album.Category.ToString();
                album.User.ToString();
                return album;
            }
        }

        public AlbumModel GetByIdForManage(int? id)
        {
            using (var session = GetSession())
            {
                var album = session.CreateQuery("from AlbumModel where Id = :id").SetParameter("id", id).UniqueResult<AlbumModel>();
                album.Category.ToString();
                album.TrustedUsers.ToString();
                album.Photos.ToString();
                album.User.ToString();
                return album;
            }
        }

        public AlbumModel GetByIdForShow(int? id)
        {
            //using (var session = GetSession())
            // {
            /*  var album = session.CreateQuery("from AlbumModel where Id = :id").SetParameter("id", id).UniqueResult<AlbumModel>();
              album.Category.ToString();
              album.TrustedUsers.ToString();
              album.Photos.ToList();
              album.User.ToString();*/
            return GetById(id, true, true, true, true, true, true);
            // return album;
            //}
        }

        public void Update(CommentModel obj)
        {
            using (var session = GetSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    session.SaveOrUpdate(obj);
                    transaction.Commit();
                }
            }
        }

        public override void Update(AlbumModel obj)
        {
            using (var session = GetSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    session.SaveOrUpdate(obj);
                    transaction.Commit();
                }
            }
        }

        public override void Delete(AlbumModel obj)
        {
            using (var session = GetSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    session.Delete(obj);
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// access to album
        /// </summary>
        /// <param name="album"></param>
        /// <param name="user"></param>
        /// <param name="passwordChecked">pass true if authorizeWithPassword(...) was called before (then it will allow access to album with password). 
        /// if set to false, method will refuse access to album with password.</param>
        /// <returns>true if user has right to view the album</returns>
        public bool IsUserAuthorizedToViewAlbum(AlbumModel album, UserModel user, bool passwordChecked)
        {
            if (album == null)
                throw new ArgumentException("album");

            // public album - eveyrone can view it
            if (album.Public)
                return true;

            // given user is the owner of the album
            if (album.User == user)
                return true;

            // given user is among trusted users for this album
            if (album.TrustedUsers != null && album.TrustedUsers.Contains(user))
                return true;

            // album has password access - allow if password was checked before
            if (album.Password != null)
                return passwordChecked;

            return false;
        }


        /// <summary>
        /// handles access to albums with password, returns true if user can see the album. should be called before IsUserAuthorizedToViewAlbum(...)
        /// </summary>
        /// <param name="album">album</param>
        /// <param name="user">user trying to access the album</param>
        /// <param name="passwordHash">hash from password which user provided, kept in session</param>
        /// <returns>
        /// true - if album has no password, or passwordHash equals password hash from album, or user is an owner
        /// false - if album has password and passwordHash doesn't equal password hash from album
        /// </returns>
        public bool authorizeWithPassword(AlbumModel album, UserModel user, string passwordHash)
        {
            if (album.User.Id == user.Id)
            {
                //user is an owner
                return true;
            }
            if (album.Password == null)
            {
                // no password
                return true;
            }
            else
            {
                if (passwordHash == album.Password)
                    //correct password
                    return true;
                else
                    // incorrect password
                    return false;
            }
        }


        // remember to fetch user with album, otherwise you get exception 
        public bool isUserAuthorizedToEditAlbum(AlbumModel album, UserModel user)
        {
            // given user is the owner of the album
            if (album.User == user)
                return true;
            else
                return false;
        }

    }
}