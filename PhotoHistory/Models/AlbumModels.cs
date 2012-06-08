using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using PhotoHistory.Data;
using NHibernate;

namespace PhotoHistory.Models
{
	 public class AlbumModel : AbstractDataModel<AlbumModel>
    {
        public virtual UserModel User { get; set; } 
        [Required]
        public virtual CategoryModel Category { get; set; }
        [Required]
        [StringLength(255, MinimumLength = 3)]
        public virtual string Name { get; set; }
        [Required]
        [StringLength(1000)]
        public virtual string Description { get; set; }
        public virtual int Rating { get; set; }
        public virtual int Views { get; set; }
        public virtual DateTime? NextNotification { get; set; }
        public virtual bool Public { get; set; }
        public virtual string Password { get; set; }
        public virtual bool CommentsAllow { get; set; }
        public virtual bool CommentsAuth { get; set; }
        public virtual int? NotificationPeriod { get; set; }

        public virtual ICollection<PhotoModel> Photos { get; set; }
        public virtual ICollection<CommentModel> Comments { get; set; }
        public virtual ICollection<UserModel> TrustedUsers{ get; set; }
        public virtual ICollection<UserModel> Followers { get; set; }
  

        // transforms an array with logins into an array of UserModels
        // returns null if at least one login was not found
        public static UserModel[] FindUsersByLogins(string[] logins)
        {
            UserRepository users = new UserRepository();
            UserModel[] userList = new UserModel[logins.Length];
            for (int i = 0; i < logins.Length; i++)
            {
                UserModel user = users.GetByUsername(logins[i]);
                if (user == null)
                    return null;
                userList[i] = user;
            }
            return userList;
        }


        // creates a record in TrustedUser table
        public virtual bool CreateTrustedUser(UserModel user)
        {
            using (var session = SessionProvider.SessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    IQuery query = session.CreateSQLQuery(string.Format("insert into trustedusers (album_id,user_id) values ({0}, {1})", Id, user.Id));
                    query.ExecuteUpdate();
                    transaction.Commit();
                }
            }
            return true;
        }


        // deletes all trusted users of the album
        public virtual void DeleteTrustedUsers()
        {
            using (var session = SessionProvider.SessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    IQuery query = session.CreateSQLQuery(string.Format("delete from trustedusers where album_id={0}", Id));
                    query.ExecuteUpdate();
                    transaction.Commit();
                }
            }
        }

		  // -------------------------------- VOTES -------------------------

		  // USE THIS INSTEAD OF 'RATING' ATTRIBUTE !
		  public virtual long getRating()
		  {
			  using ( var session = SessionProvider.SessionFactory.OpenSession() )
			  {
				  long positive_votes = session.CreateSQLQuery( string.Format( "SELECT count(*) FROM votes WHERE album_id={0} AND up=true", Id ) ).UniqueResult<long>();
				  long negative_votes = session.CreateSQLQuery( string.Format( "SELECT count(*) FROM votes WHERE album_id={0} AND up=false", Id ) ).UniqueResult<long>();
				  return positive_votes - negative_votes;
			  }
		  }
		  // create a vote
		  public virtual bool CreateVote(UserModel user, bool up)
		  {
			  using ( var session = SessionProvider.SessionFactory.OpenSession() )
			  {
				  using ( var transaction = session.BeginTransaction() )
				  {
					  try
					  {
						  IQuery query = session.CreateSQLQuery( string.Format( "insert into votes (album_id,user_id,up) values ({0}, {1}, {2})", Id, user.Id, up ) );
						  query.ExecuteUpdate();
						  transaction.Commit();
					  }
					  catch ( Exception )
					  {
						  // vote was not created
						  return false;
					  }
				  }
			  }
			  return true;
		  }

		 // -------------------------------- END VOTES -------------------------
    }

    public class AlbumProfileModel
    {
        [Required]
        public virtual int? Id { get; set; }
        [Required]
        public virtual string Name { get; set; }
        [Required]
        public virtual string StartDate { get; set; }
        [Required]
        public virtual string EndDate { get; set; }
        [Required]
        public virtual int Views { get; set; }
        [Required]
        public virtual int Rating { get; set; }
        [Required]
        public virtual int Comments { get; set; }
        [Required]
        public virtual List<string> Thumbnails { get; set; }

        public static AlbumProfileModel FromAlbum(AlbumModel album)
        { 
            string start,end;
            Helpers.AlbumDateRange(album, out start, out end);
            AlbumProfileModel profileAlbum = new AlbumProfileModel()
            {
                Id = album.Id,
                Name = album.Name,
                Thumbnails = Helpers.AlbumThumbnails(album),
                StartDate = start,
                EndDate = end,
                Views = album.Views,
                Rating = album.Rating,
                
            };
            return profileAlbum;
        }
    }

    public class HomepageAlbumModel
    {
        [Required]
        public virtual string Name { get; set; }

        [Required]
        public virtual List<AlbumProfileModel> Albums {get; set;}
    }


    public class AlbumModelSortedPhotos : AlbumModel
    {
        public virtual List<PhotoModel> SortedPhotos { get; set; }

        public AlbumModelSortedPhotos(AlbumModel album)
        {
            SortedPhotos = new List<PhotoModel>(album.Photos);
            SortedPhotos.Sort((x, y) => x.Date.CompareTo(y.Date));
        }
    }

    public class BrowseAlbumModel
    {
        public int SelectedCategory { get; set; }
        public int CurrentPage { get; set; } 
        public int PageCount { get; set; }
        public List<CategoryModel> Categories { get; set; }
        public List<AlbumProfileModel> Albums { get; set; }


    }
}