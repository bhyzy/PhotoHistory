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
		public virtual string ActivationCode { get; set; }
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
		[UniqueUsername( ErrorMessage = "Specified username is already taken." )]
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
		[ValidateEmail( ErrorMessage = "Specified e-mail address is invalid." )]
		[UniqueEmail( ErrorMessage = "Specified e-mail address is already used by another account." )]
		[Display( Name = "E-mail address" )]
		public virtual string Email { get; set; }
	}

	public class SignInUserModel
	{
		[Required]
		[Display( Name = "Username" )]
		public virtual string Login { get; set; }

		[Required]
		[DataType( DataType.Password )]
		public virtual string Password { get; set; }

		[Display( Name = "Remember me" )]
		public virtual bool Remember { get; set; }
	}

	public class UserSettingsModel
	{
		[DateYearInRange( 1900, 2012, ErrorMessage = "Year of birth is not in range 1900-2012" )]
		[Display( Name = "Date of birth" )]
		public virtual DateTime? DateOfBirth { get; set; }

		[StringLength( 1024 )]
		[Display( Name = "About me" )]
		public virtual string About { get; set; }

		//[Required]
		public virtual bool NotifyComment { get; set; }

		//[Required]
		public virtual bool NotifyPhoto { get; set; }

		//[Required]
		public virtual bool NotifySubscription { get; set; }
	}
}
