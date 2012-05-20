using PhotoHistory.Data;
using PhotoHistory.Models;

namespace PhotoHistory.API.Authentication
{
	public static class UserAuthentication
	{
		// Tries to authenticate given user credentials.
		// Returns empty string on success or error message on failure.
		public static string TryCredentials(string userName, string password)
		{
			if ( string.IsNullOrEmpty(userName) )
				return "empty username";
			if ( string.IsNullOrEmpty(password))
				return "empty password";

			UserRepository userRepository = new UserRepository();
			UserModel user = userRepository.GetByUsername( userName );

			if ( user == null )
				return "wrong username";
			if ( user.ActivationCode != null )
				return "account not activated";
			if ( user.Password != password.HashMD5() )
				return "wrong password";

			// credentials successfully authenticated
			return string.Empty; 
		}
	}
}