using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using PhotoHistory.Data;
using NHibernate;

namespace PhotoHistory.Models
{
    public class AlbumModel
    {
        public virtual int? Id { get; set; }
        //public virtual int UserId { get; set; }
        public virtual UserModel User { get; set; } // 'belongs to' association

        [Required]
        public virtual CategoryModel Category { get; set; }
        //public virtual int CategoryId { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 3)]
        public virtual string Name { get; set; }

        [Required]
        [StringLength(1000)]
        public virtual string Description { get; set; }

        public virtual int Rating { get; set; }
        public virtual DateTime? NextNotification { get; set; }
        public virtual bool Public { get; set; }
        public virtual string Password { get; set; }
        public virtual bool CommentsAllow { get; set; }
        public virtual bool CommentsAuth { get; set; }



        // transforms an array with logins into an array of UserModels
        // returns null if at least one login was not found
        public static UserModel[] FindUsersByLogins(string[] logins)
        {
            UserRepository users = new UserRepository();
            UserModel[] userList = new UserModel[logins.Length];
            for (int i=0;i<logins.Length;i++)
            {
                UserModel user = users.GetByUsername(logins[i]);
                if (user==null)
                    return null;
                userList[i] = user;
            }
            return userList;
        }


        // creates a record in TrustedUser table
        public virtual bool CreateTrustedUser(UserModel user)
        {        
            using ( var session = SessionProvider.SessionFactory.OpenSession() )
			{
				using ( var transaction = session.BeginTransaction() )
				{
                    IQuery query = session.CreateSQLQuery(string.Format("insert into trustedusers (album_id,user_id) values ({0}, {1})",Id,user.Id) );
                    //query.SetParameter( "album_id", Id );
                    //query.SetParameter( "user_id", user.Id );
					query.ExecuteUpdate();
					transaction.Commit();
				}
			}
            return true;            
        }

    }



    public class AlbumProfileModel
    {
        public virtual string Name {get; set;}
        public virtual string TimeSpan { get; set; }
        public virtual int? Views { get; set; }
        

    }
}