using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PhotoHistory.Models;

namespace PhotoHistory.Data
{
    public class CategoryRepository : DataRepository<CategoryModel, Int32?>
    {
        public override void Create(CategoryModel obj)
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

        public override CategoryModel GetById(int? id)
        {
            using (var session = GetSession())
            {
                return session.CreateQuery("from CategoryModel where Id = :id").SetParameter("id", id).UniqueResult<CategoryModel>();
            }
        }

        public  CategoryModel GetByName(String name)
        {
            using (var session = GetSession())
            {
                return session.CreateQuery("from CategoryModel where Id = :id").SetParameter("name", name).UniqueResult<CategoryModel>();
            }
        }

        public override void Update(CategoryModel obj)
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

        public override void Delete(CategoryModel obj)
        {
            using (var session = GetSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    session.CreateQuery("delete from CategoryModel where Id = :id").SetParameter("id", obj.Id).ExecuteUpdate();
                    transaction.Commit();
                }
            }
        }

        public List<CategoryModel> getCategories()
        {
            using (var session = GetSession())
            {
                List<CategoryModel> categories=session.CreateQuery("from CategoryModel").Enumerable<CategoryModel>().ToList<CategoryModel>();
                foreach (CategoryModel cat in categories)
                {
                    cat.Id.ToString();
                    cat.Name.ToString();
                }
                
                return categories;
            }
        }

    }

}