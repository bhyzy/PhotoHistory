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
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<AlbumModel> GetByCategory(CategoryModel category, bool withUser = false, bool withPhotos = false, bool withComments = false,
              bool withCategory = false, bool withTrustedUsers = false, bool withFollowers = false)
        {
            using (var session = GetSession())
            {
                List<AlbumModel> albums =session.CreateQuery("from AlbumModel where Category = :cat").SetParameter("cat", category).Enumerable<AlbumModel>().ToList<AlbumModel>();
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
					return session.CreateQuery( "from AlbumModel where Id = :id" ).SetParameter( "id", id ).UniqueResult<AlbumModel>();
            }
        }

		  public AlbumModel GetById(int? id,
			  bool withUser = false, bool withPhotos = false, bool withComments = false, 
			  bool withCategory = false, bool withTrustedUsers = false)
		  {
			  using ( var session = GetSession() )
			  {
				  AlbumModel album = session.CreateQuery( "from AlbumModel where Id = :id" ).SetParameter( "id", id ).UniqueResult<AlbumModel>();
				  if ( album != null )
				  {
                      if (withComments) album.Comments.ToList().Sort(delegate(CommentModel a, CommentModel b)
                          {
                              return a.Date.CompareTo(b.Date);
                          });
					  if ( withUser ) album.User.ToString();
					  if ( withPhotos ) album.Photos.ToString();
					  if ( withCategory ) album.Category.ToString();
					  if ( withTrustedUsers ) album.TrustedUsers.ToList();
				  }
				  return album;
			  }
		  }

        public AlbumModel GetWithComments(int? id)
        {
			  return GetById( id, withComments: true );
        }

        public AlbumModel GetByIdWithPhotos(int? id, bool with)
        {
			  return GetById( id, withPhotos: true );
        }

        public AlbumModel GetByIdWithUser(int? id)
        {
			  return GetById( id, withUser: true );
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
                album.Category.ToString();
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
            //using (var session = GetSession())
           // {
              /*  var album = session.CreateQuery("from AlbumModel where Id = :id").SetParameter("id", id).UniqueResult<AlbumModel>();
                album.Category.ToString();
                album.TrustedUsers.ToString();
                album.Photos.ToList();
                album.User.ToString();*/
                return GetById(id, true, true, true, true, true);
               // return album;
            //}
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