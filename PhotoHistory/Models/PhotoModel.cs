using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PhotoHistory.Models
{
    public class PhotoModel
    {
        public virtual int? Id { get; set; }
        public virtual AlbumModel Album { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual string Description { get; set; }
    }

    public class NewPhotoModel
    {
        [Required]
        public virtual AlbumModel Album { get; set; }
        [Required]
        public virtual DateTime Date { get; set; }
        [StringLength(1000)]
        public virtual string Description { get; set; }

    }
}