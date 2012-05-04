using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PhotoHistory.Models;

namespace PhotoHistory.Data
{
    public class PhotoRepository : DataRepository<PhotoModel, Int32?>
    {
        public override void Create(PhotoModel obj)
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

        public override PhotoModel GetById(int? id)
        {
            using (var session = GetSession())
            {
                return session.CreateQuery("from PhotoModel where Id = :id").SetParameter("id", id).UniqueResult<PhotoModel>();
            }
        }

        public override void Update(PhotoModel obj)
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

        public override void Delete(PhotoModel obj)
        {
            using (var session = GetSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    session.CreateQuery("delete from PhotoModel where Id = :id").SetParameter("id", obj.Id).ExecuteUpdate();
                    transaction.Commit();
                }
            }
        }
    }
}