using System.ComponentModel.DataAnnotations;
using PhotoHistory.Data;

namespace PhotoHistory
{
	public class UniqueUsernameAttribute : ValidationAttribute
	{
		public override bool IsValid(object value)
		{
			UserRepository userRepository = new UserRepository();
			return userRepository.GetByUsername( value as string ) == null;
		}
	}

	public class UniqueEmailAttribute : ValidationAttribute
	{
		public override bool IsValid(object value)
		{
			UserRepository userRepository = new UserRepository();
			return userRepository.GetByEmail( value as string ) == null;
		}
	}

	public class ValidateEmailAttribute : RegularExpressionAttribute
	{
		public ValidateEmailAttribute() :
			base( @"^(([A-Za-z0-9]+_+)|([A-Za-z0-9]+\-+)|([A-Za-z0-9]+\.+)|([A-Za-z0-9]+\++))*[A-Za-z0-9]+@((\w+\-+)|(\w+\.))*\w{1,63}\.[a-zA-Z]{2,6}$" ) { }
	} 
}

