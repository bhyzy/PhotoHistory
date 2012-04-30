using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PhotoHistory.Data;

namespace PhotoHistory.Models
{
    public class CategoryModel
    {

        public virtual int? Id { get; set; }
        public virtual string Name { get; set; }

        
        // fills category table with a few default categories
        public static void EnsureStartingData()
        {
            using (var session = SessionProvider.SessionFactory.OpenSession())
            {
                int category_count = session.QueryOver<CategoryModel>().RowCount();
                if (category_count == 0)
                {
                    CategoryModel[] categories = new CategoryModel[6]; 
                    categories[0] = new CategoryModel(){Name = "People"};
                    categories[1] = new CategoryModel() { Name = "Building" };
                    categories[2] = new CategoryModel() { Name = "City" };
                    categories[3] = new CategoryModel() { Name = "Landscape" };
                    categories[4] = new CategoryModel() { Name = "Room" };
                    categories[5] = new CategoryModel() { Name = "Other" };

                    using (var trans = session.BeginTransaction())
                    {
                        foreach (var cat in categories)
                            session.Save(cat);
                        trans.Commit();
                    }
                }
            }

        }

    }
}