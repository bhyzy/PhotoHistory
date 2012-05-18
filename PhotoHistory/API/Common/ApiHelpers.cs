using System.Text;
using System;

namespace PhotoHistory.API.Common
{
	public static class ApiHelpers
	{
		public static string[] ParseBasicAuthHeader(string authHeader)
		{
			// Check this is a Basic Auth header
			if ( authHeader == null || authHeader.Length == 0 || !authHeader.StartsWith( "Basic" ) ) 
				return null;

			string[] credentials;
			try
			{
				// Pull out the Credentials with are seperated by ':' and Base64 encoded
				string base64Credentials = authHeader.Substring( 6 );
				credentials = Encoding.ASCII.GetString(
					Convert.FromBase64String( base64Credentials ) ).Split( new char[] { ':' } );
			}
			catch (Exception)
			{
				return null;
			}

			if ( credentials.Length != 2 || string.IsNullOrEmpty( credentials[0] ) || string.IsNullOrEmpty( credentials[0] ) )
				return null;

			// Okay this is the credentials
			return credentials;
		}
	}
}