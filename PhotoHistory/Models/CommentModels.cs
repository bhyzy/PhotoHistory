using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoHistory.Models
{
    public class CommentModel
    {
        public virtual int? Id { get; set; }
        public virtual AlbumModel Album { get; set; }
        public virtual UserModel User { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual String Body { get; set; }
    }
}