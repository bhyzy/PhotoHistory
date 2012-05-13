using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Web.Helpers;
using System.Net.Mail;
using System.Net;
using System.Web.Mvc;
using PhotoHistory.Models;
using PhotoHistory.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;

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

        public static Image TransformWithAspectRatio(Image image, int maxWidth, int maxHeight,bool thumbnail)
        {
            if (image == null)
                return null;

            if (image.Width <= maxWidth && image.Height <= maxHeight)
                return (Image)image.Clone();

            int newWidth = 0;
            int newHeight = 0;

            if (thumbnail)
            {
                newWidth = maxWidth;
                newHeight = maxHeight;
            }
            else
            {
                var ratioX = (double)maxWidth / image.Width;
                var ratioY = (double)maxHeight / image.Height;
                var ratio = Math.Min(ratioX, ratioY);

                newWidth = (int)(image.Width * ratio);
                newHeight = (int)(image.Height * ratio);
            }

            var newImage = new Bitmap(newWidth, newHeight);

            System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(newImage);

            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.SmoothingMode = SmoothingMode.HighQuality;
            graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphic.CompositingQuality = CompositingQuality.HighQuality;

            graphic.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }

        public static int GetAge(DateTime birthDate)
        {
            DateTime now = DateTime.Today;
            int age = now.Year - birthDate.Year;
            if (now.Month < birthDate.Month || (now.Month == birthDate.Month && now.Day < birthDate.Day)) age--;
            return age;
        }

        public static List<string> AlbumThumbnails(AlbumModel album)
        {
            List<PhotoModel> photos = new List<PhotoModel>(album.Photos);
            photos.Sort(delegate(PhotoModel a, PhotoModel b)
            {
                return a.Date.CompareTo(b.Date);
            }); 
            List<string> result = new List<string>(photos.Count);
            foreach (PhotoModel photo in photos)
            {
                result.Add(PhotoThumbnail(photo));
            }
            return result;
        }

        public static string PhotoThumbnail(PhotoModel photo)
        {
            return Path.GetDirectoryName(photo.Path).Replace("\\", "/") + "/" + Path.GetFileNameWithoutExtension(photo.Path) + "_mini.jpg";
        }

        public static List<AlbumProfileModel> Convert(List<AlbumModel> albums)
        {
            List<AlbumProfileModel> list = new List<AlbumProfileModel>();
            string start,end;

            foreach (AlbumModel album in albums)
            {
                Helpers.AlbumDateRange(album, out start, out end);

                AlbumProfileModel profileAlbum = new AlbumProfileModel()
                {
                    Id = album.Id,
                    Name = album.Name,
                    Thumbnails = Helpers.AlbumThumbnails(album),
                    StartDate = start,
                    EndDate = end,
                    Views = album.Views,
                    Comments = album.Comments.Count,
                    Rating = album.Rating
                };
                list.Add(profileAlbum);
            }
            return list;
            
        }
        
        public static void AlbumDateRange(AlbumModel album, out string start, out  string end)
        {
            start = "";
            end = "";

            if (album.Photos.Count == 0)
                return;

            DateTime startD = album.Photos.First().Date;
            DateTime endD = album.Photos.First().Date;
            foreach (PhotoModel photo in album.Photos)
            {
                if (photo.Date < startD)
                    startD = photo.Date;
                else if (photo.Date > endD)
                    endD = photo.Date;
            }

            start = startD.ToString("dd/MM/yyyy");
            end = endD.ToString("dd/MM/yyyy");
        }
    }

    public enum ChartCategory {Popular, TopRated, Biggest, MostComments}
    public enum MainCategory { Home, Browse, Charts}
}
