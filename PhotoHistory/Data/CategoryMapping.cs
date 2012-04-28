using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using PhotoHistory.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace PhotoHistory.Data
{
    public class CategoryMapping : ClassMapping<CategoryModel>
    {

        public CategoryMapping()
        {
            Table("Categories");

            Id(x => x.Id, map =>
            {
                map.Column("category_id");
                map.Generator(Generators.Sequence, g => g.Params(new { sequence = "categories_category_id_seq" }));
            });
            
            Property(x => x.Name, map =>
            {
                map.Column("name");
                map.Length(255);
                map.NotNullable(true);
            });

        }
    }
}