using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace PhotoHistory
{
	public static class Helpers
	{
		public static string HashMD5(this string stringToHash)
		{
			MD5 md5 = new MD5CryptoServiceProvider();
			Byte[] originalBytes = ASCIIEncoding.Default.GetBytes( stringToHash );
			Byte[] encodedBytes = md5.ComputeHash( originalBytes );
			return BitConverter.ToString( encodedBytes );
		}
	}
}
