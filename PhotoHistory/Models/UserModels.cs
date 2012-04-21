using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PhotoHistory.Models
{
	public class UserModel
	{
		public virtual int? Id { get; set; }
		public virtual string Login { get; set; }
		public virtual string Password { get; set; }
		public virtual string Email { get; set; }
		public virtual bool Active { get; set; }
		public virtual DateTime? DateOfBirth { get; set; }
		public virtual string About { get; set; }
		public virtual bool NotifyComment { get; set; }
		public virtual bool NotifyPhoto { get; set; }
		public virtual bool NotifySubscription { get; set; }
	}

	public class NewUserModel
	{
		[Required]
		[StringLength( 100, MinimumLength = 3 )]
		[UniqueUsername]
		[Display( Name = "Username" )]
		public virtual string Login { get; set; }

		[Required]
		[StringLength( 100, MinimumLength = 3 )]
		[DataType( DataType.Password )]
		public virtual string Password { get; set; }

		[Required]
		[Compare( "Password" )]
		[DataType( DataType.Password )]
		[Display( Name = "Confirm password" )]
		public virtual string ConfirmedPassword { get; set; }

		[Required]
		[StringLength( 255 )]
		[DataType( DataType.EmailAddress )]
		[UniqueEmail]
		[Display( Name = "E-mail address" )]
		public virtual string Email { get; set; }
	}
}
