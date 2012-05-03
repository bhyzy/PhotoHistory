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

        public virtual ICollection<AlbumModel> Albums { get; set; }
        
        /*
        public UserModel()
        {
            Albums = new List<AlbumModel>();
        }*/
         
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

	public abstract class ChangeUserPasswordModel
	{
		[Required]
		[StringLength( 100, MinimumLength = 3 )]
		[DataType( DataType.Password )]
		[Display( Name = "New password" )]
		public virtual string NewPassword { get; set; }

		[Required]
		[Compare( "NewPassword" )]
		[DataType( DataType.Password )]
		[Display( Name = "Confirm new password" )]
		public virtual string ConfirmedNewPassword { get; set; }
	}

	public class ChangeExistingUserPasswordModel : ChangeUserPasswordModel
	{
		[Required]
		[DataType( DataType.Password )]
		[MatchCurrentPassword( ErrorMessage = "Specified password doesn't match your account's password" )]
		[Display( Name = "Current password" )]
		public virtual string CurrentPassword { get; set; }
	}

	public class ResetUserPasswordModel : ChangeUserPasswordModel
	{
		[Required]
		public virtual string Username { get; set; }
		[Required]
		public virtual string Token { get; set; }
	}

	public class RestoreUserPasswordModel
	{
		[Required]
		[ValidUsernameOrEmailAttribute( ErrorMessage = "Neither an username nor an e-mail address matching this could be found" )]
		[Display( Name = "Account" )]
		public virtual string Account { get; set; }
	}

    public class UserProfileModel
    {   
        [Required]
        public virtual string Name { get; set; }
        [Required]
        public virtual string Age { get; set; }
        [Required]
        public virtual string About { get; set; }
        [Required]
        public virtual IEnumerable<AlbumProfileModel> Albums { get; set; }
    }
}
