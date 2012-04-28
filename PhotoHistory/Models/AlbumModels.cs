using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PhotoHistory.Models
{
    public class AlbumModel
    {
        public virtual int? Id { get; set; }
        public virtual int UserId { get; set; }
        public virtual UserModel User { get; set; } // 'belongs to' association

        [Required]
        public virtual int CategoryId { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 3)]
        public virtual string Name { get; set; }

        [Required]
        [StringLength(1000)]
        public virtual string Description { get; set; }

        public virtual int Rating { get; set; }
        public virtual DateTime? NextNotification { get; set; }
        public virtual bool Public { get; set; }
        public virtual string Password { get; set; }
        public virtual bool CommentsAllow { get; set; }
        public virtual bool CommentsAuth { get; set; }
    }




}