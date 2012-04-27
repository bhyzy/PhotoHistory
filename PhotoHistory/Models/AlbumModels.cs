using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoHistory.Models
{
    public class AlbumModel
    {
        public virtual int? Id { get; set; }
        public virtual int UserId { get; set; }
        public virtual UserModel User { get; set; } // 'belongs to' association
        public virtual int CategoryId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual int Rating { get; set; }
        public virtual DateTime? NextNotification { get; set; }
        public virtual bool Public { get; set; }
        public virtual string Password { get; set; }
        public virtual bool CommentsAllow { get; set; }
        public virtual bool CommentsAuth { get; set; }
    }



}