using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoHistory.Models
{
	public class CommentModel : AbstractDataModel<CommentModel>
    {
        public virtual AlbumModel Album { get; set; }
        public virtual UserModel User { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual String Body { get; set; }
    }

    public class NewCommentModel
    {
        public String Message { get; set; }
        public String Body { get; set; }
        public String Date { get; set; }
        public String UserName { get; set; }
    }
}