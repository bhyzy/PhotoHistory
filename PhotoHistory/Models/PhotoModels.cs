﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PhotoHistory.Models
{
	public class PhotoModel : AbstractDataModel<PhotoModel>
	{
		public virtual AlbumModel Album { get; set; }
		public virtual DateTime Date { get; set; }
		public virtual string Description { get; set; }
		public virtual string Path { get; set; }
		//public virtual string Location { get; set; }
		public virtual double? LocationLatitude { get; set; }
		public virtual double? LocationLongitude { get; set; }
	}


	public class NewPhotoModel
	{
		[Required( ErrorMessage = "You must select an album." )]
		public virtual int? AlbumId { get; set; }

		[Required( ErrorMessage = "You must specify date." )]
		[ValidPhotoDate( ErrorMessage = "Enter valid date. You can't select date from the future." )]
		public virtual String Date { get; set; }

		[StringLength( 1000, ErrorMessage = "Description can't be longer than 1000 characters." )]
		public virtual string Description { get; set; }

		[Required( ErrorMessage = "You must select source for your photo." )]
		public virtual string Source { get; set; }

		public virtual string PhotoURL { get; set; }

		[ValidUpload]
		public virtual HttpPostedFileBase FileInput { get; set; }

	}
}