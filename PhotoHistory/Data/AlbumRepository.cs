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
        }

        public override void Delete(AlbumModel obj)
        {
        }


    }
}