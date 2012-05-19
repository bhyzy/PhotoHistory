using System;
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

        public override AlbumModel GetById(int? id)
        {
            using (var session = GetSession())
            {
                return session.CreateQuery("from AlbumModel where Id = :id").SetParameter("id", id).UniqueResult<AlbumModel>();
            }
        }

        public AlbumModel GetWithComments(int? id)
        {
            using (var session = GetSession())
            {
                AlbumModel album = session.CreateQuery("from AlbumModel where Id = :id").SetParameter("id", id).UniqueResult<AlbumModel>();
                album.Comments.ToList();
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

        public AlbumModel GetByIdWithUser(int? id)
        {
            using (var session = GetSession())
            {
                var album = session.CreateQuery("from AlbumModel where Id = :id").SetParameter("id", id).UniqueResult<AlbumModel>();
                album.User.ToString();
                return album;
            }
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
                IList<object[]> list = sql.List<object[]>();
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
                var albumsId =albumQuery.List<int>();

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
                album.Photos.ToList();
                album.User.ToString();
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


		  public ICollection<AlbumModel> GetAll()
		  {
			  using ( var session = GetSession() )
			  {
				  return session.CreateQuery( "from AlbumModel" ).List<AlbumModel>();
			  }
		  }

		  public bool IsUserAuthorizedToViewAlbum(AlbumModel album, UserModel user)
		  {
			  if ( album == null )
				  throw new ArgumentException( "album" );

			  // public album - eveyrone can view it
			  if ( album.Public )
				  return true;

			  // given user is the owner of the album
			  if ( album.User == user )
				  return true;

			  // given user is among trusted users for this album
			  if ( album.TrustedUsers != null && album.TrustedUsers.Contains( user ) )
				  return true;

			  return false;
		  }
    }
}