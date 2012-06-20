using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PhotoHistory.API.Models
{
	public class NewPhotoModel
	{
		public virtual int AlbumID { get; set; }
		public virtual DateTime Date { get; set; }
		public virtual string Description { get; set; }
        public virtual double? LocationLatitude { get; set; }
        public virtual double? LocationLongitude { get; set; }
		public virtual string Image { get; set; } // base64-encoded
	}
}