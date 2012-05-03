using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NHibernate.Mapping.ByCode.Conformist;
using PhotoHistory.Models;
using NHibernate.Mapping.ByCode;

namespace PhotoHistory.Data
{
    public class PhotoMapping:ClassMapping<PhotoModels>
    {
        public PhotoMapping()
        {
            Table("Photos");

            Id(x => x.Id, map =>
            {
                map.Column("photo_id");
                map.Generator(Generators.Sequence, g => g.Params(new { sequence = "photos_photo_id_seq" }));
            });
            
           ManyToOne(x => x.Album, map =>
           {
                    map.Column("album_id");
                    map.NotNullable(true);
           });

            Property(x=> x.Date, map=>
            {
                map.Column("date_taken");
                map.NotNullable(true);
            });

            Property(x=> x.Description, map=>
            {
                map.Column("description");
                map.NotNullable(false);
            });
        }
    }

    
}