using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NHibernate.Mapping.ByCode.Conformist;
using PhotoHistory.Models;
using NHibernate.Mapping.ByCode;

namespace PhotoHistory.Data
{
    public class CommentMapping:ClassMapping<CommentModel>
    {
        public CommentMapping()
        {
            Table("Comments");

            Id(x => x.Id, map =>
            {
                map.Column("comment_id");
                map.Generator(Generators.Sequence, g => g.Params(new { sequence = "comments_comment_id_seq" }));
            });

            ManyToOne(x => x.Album, map =>
            {
                map.Column("album_id");
                map.NotNullable(true);
            });

           ManyToOne(x => x.User, map =>
           {
               map.Column("user_id");
               map.NotNullable(true);
           });

            Property(x=> x.Date, map=>
            {
                map.Column("date_posted");
                map.NotNullable(true);
            });

            Property(x=> x.Body, map=>
            {
                map.Column("body");
                map.NotNullable(false);
            });

        }
    }
}