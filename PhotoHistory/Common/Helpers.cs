using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Web.Helpers;
using System.Net.Mail;
using System.Net;
using System.Web.Mvc;

namespace PhotoHistory
{
	public static class Helpers
	{
		public static string HashMD5(this string stringToHash)
		{
			MD5 md5 = new MD5CryptoServiceProvider();
			Byte[] originalBytes = ASCIIEncoding.Default.GetBytes( stringToHash );
			Byte[] encodedBytes = md5.ComputeHash( originalBytes );
			return BitConverter.ToString( encodedBytes ).Replace( "-", "" );
		}

		public static void SendEmail(string to, string subject, string body)
		{
			WebMail.SmtpServer = "smtp.gmail.com";
			WebMail.SmtpPort = 587;
			WebMail.EnableSsl = true;
			WebMail.UserName = "pastexplorer@gmail.com";
			WebMail.Password = "pastexplorer666";
			WebMail.From = "pastexplorer@gmail.com";
			WebMail.SmtpUseDefaultCredentials = false;

			WebMail.Send( to, subject, body, isBodyHtml: true );
		}

		public static MvcHtmlString MyValidationMessage(this HtmlHelper helper, string fieldName)
		{
			MvcHtmlString validationMsg = System.Web.Mvc.Html.ValidationExtensions.ValidationMessage( helper, fieldName );
			if ( validationMsg != null )
			{
				return new MvcHtmlString( string.Format( "<span class=\"help-inline\">{0}</span>", validationMsg.ToString() ) );
			}

			return new MvcHtmlString( string.Empty );
		}

		public static MvcHtmlString MyValidationMark(this HtmlHelper helper, string fieldName)
		{
			MvcHtmlString validationMsg = System.Web.Mvc.Html.ValidationExtensions.ValidationMessage( helper, fieldName );
			if ( validationMsg != null )
			{
				return new MvcHtmlString( "error" );
			}

			return new MvcHtmlString( string.Empty );
		}

        public static int GetAge(DateTime birthDate)
        {
            DateTime now = DateTime.Today;
            int age = now.Year - birthDate.Year;
            if (now.Month < birthDate.Month || (now.Month == birthDate.Month && now.Day < birthDate.Day)) age--;
            return age;
        }
    }
}
