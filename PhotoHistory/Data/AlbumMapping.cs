﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PhotoHistory.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;


using System.Text;

namespace PhotoHistory.Data
{
    public class AlbumMapping : ClassMapping<AlbumModel>
    {
        public AlbumMapping()
        {

            Table("Albums");
            Id(x => x.Id, map =>
            {
                map.Column("album_id");
                map.Generator(Generators.Sequence, g => g.Params(new { sequence = "albums_album_id_seq" }));
            });
            // 'belongs to' association
            ManyToOne(x => x.User, map =>
                {
                    map.Column("user_id");
                });
            Property(x => x.UserId, map =>
                {
                    map.Column("user_id");
                    map.NotNullable(true);
                });
            Property(x => x.CategoryId, map =>
            {
                map.Column("category_id");
                map.NotNullable(true);
            });
            Property(x => x.Name, map =>
            {
                map.Column("name");
                map.Length(255);
                map.NotNullable(true);
            });
            Property(x => x.Description, map =>
            {
                map.Column("description");
                map.Length(1000);
            });
            Property(x => x.Rating, map =>
            {
                map.Column("rating");
            });
            Property(x => x.NextNotification, map =>
            {
                map.Column("next_notification");
            });
            Property(x => x.Public, map =>
            {
                map.Column("public");
                map.NotNullable(true);
            });
            Property(x => x.Password, map =>
            {
                map.Column("password");
                map.Length(255);
                map.NotNullable(true);
            });
            Property(x => x.CommentsAllow, map =>
            {
                map.Column("comments_allow");
                map.NotNullable(true);
            });
            Property(x => x.CommentsAuth, map =>
            {
                map.Column("comments_auth");
                map.NotNullable(true);
            });                                   
        }
    }
}