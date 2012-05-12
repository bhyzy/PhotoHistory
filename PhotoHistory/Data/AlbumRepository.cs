using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PhotoHistory.Models;
using NHibernate;

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

        public override AlbumModel GetById(int? id)
        {
            using (var session = GetSession())
            {
                return session.CreateQuery("from AlbumModel where Id = :id").SetParameter("id", id).UniqueResult<AlbumModel>();
            }
        }

        public AlbumModel GetByIdWithUser(int? id)
        {
            using (var session = GetSession())
            {
                var album = session.CreateQuery("from AlbumModel where Id = :id").SetParameter("id", id).UniqueResult<AlbumModel>();
                album.User.ToString();
                return album;
            }
        }

        public AlbumModel GetByIdForEdit(int? id)
        {
            using (var session = GetSession())
            {
                var album = session.CreateQuery("from AlbumModel where Id = :id").SetParameter("id", id).UniqueResult<AlbumModel>();
                album.TrustedUsers.ToList();
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
                return album;
            }
        }

        public AlbumModel GetByIdForShow(int? id)
        {
            using (var session = GetSession())
            {
                var album = session.CreateQuery("from AlbumModel where Id = :id").SetParameter("id", id).UniqueResult<AlbumModel>();
                album.Category.ToString();
                album.TrustedUsers.ToString();
                album.Photos.ToList().Sort( (x,y) => x.Date.CompareTo(y.Date)  );
                album.User.ToString();
                return album;
            }
        }

        public AlbumModel GetByIdWithPhotos(int? id)
        {
            using (var session = GetSession())
            {
                AlbumModel album = session.CreateQuery("from AlbumModel where Id = :id").SetParameter("id", id).UniqueResult<AlbumModel>();
                album.Photos.ToList();
                return album;
            }
        }

        /*public IEnumerable<AlbumModel> GetByUser(int ?userID)
        {
            using (var session = GetSession())
            {
                UserRepository repo = new UserRepository();
                UserModel user = repo.GetById(userID);
                return session.CreateQuery("from AlbumModel where User= :user").SetParameter("user", userID).Enumerable<AlbumModel>();
            }

        }*/

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


    }
}