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
using System.Web;
using System.Security.Policy;
using System.Threading;

namespace PhotoHistory
{
    public static class Helpers
    {
        public static string HashMD5(this string stringToHash)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            Byte[] originalBytes = ASCIIEncoding.Default.GetBytes(stringToHash);
            Byte[] encodedBytes = md5.ComputeHash(originalBytes);
            return BitConverter.ToString(encodedBytes).Replace("-", "");
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

            WebMail.Send(to, subject, body, isBodyHtml: true);
        }

        public static bool isFollower(AlbumModel album, UserModel user)
        {
            foreach (UserModel follower in album.Followers)
            {
                if (follower.Id == user.Id)
                    return true;
            }
            return false;
        }

        public static void NotifyAlbumObservers(AlbumModel album)
        {
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            Uri requestUrl = url.RequestContext.HttpContext.Request.Url;
            foreach (UserModel follower in album.Followers)
            {
                if (follower.NotifySubscription)
                {
                    string link = string.Format( "{0}://{1}{2}", requestUrl.Scheme, requestUrl.Authority,
					url.Action("Show", "Album", new { id = album.Id }) );
                    SendEmail(follower.Email, string.Format("{0} has added new photo", album.User.Login),
                        string.Format("Dear {0},<br/>{1} has added new photo. If you want to see updates please follow this <a href=\"{2}\">link</a>",
                            follower.Login, album.User.Login, link)
                   );
                }
            }
        }

        public static void SendEmailContextFree(String to, String subject, String body)
        {
            var fromAddress = new MailAddress("pastexplorer@gmail.com", "PastExplorer");
            var toAddress = new MailAddress(to, to);

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, "pastexplorer666")
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
            }

        }

        public static void NotifyCommentObserver(CommentModel comment)
        {
            AlbumModel album = comment.Album;
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            Uri requestUrl = url.RequestContext.HttpContext.Request.Url;
            //Don't send notification if comment belongs to album's owner or if notification is turned off
            if (album.User.NotifyComment && album.User.Id!= comment.User.Id)
            {
                string link = string.Format("{0}://{1}{2}", requestUrl.Scheme, requestUrl.Authority,
                    url.Action("Show", "Album", new { id = album.Id })) +"#comment"+comment.Id;
                string body = string.Format("{0} has added comment to your album {1}:</br><i>{2}</i></br></br> To see your album visit this <a href='{3}'>link.</a>",
                    comment.User.Login, album.Name, comment.Body,link);
                MailSendJob mail = new MailSendJob();
                mail.to = album.User.Email;
                mail.subject = string.Format("{0} has added comment to your album" , comment.User.Login);
                mail.body = body;
                ThreadStart job = new ThreadStart(mail.send);
                Thread thread = new Thread(job);
                thread.Start();

            }
        }

        public static void RemindPhoto(AlbumModel album)
        {
        }

        public static MvcHtmlString MyValidationMessage(this HtmlHelper helper, string fieldName)
        {
            MvcHtmlString validationMsg = System.Web.Mvc.Html.ValidationExtensions.ValidationMessage(helper, fieldName);
            if (validationMsg != null)
            {
                return new MvcHtmlString(string.Format("<span class=\"help-inline\">{0}</span>", validationMsg.ToString()));
            }

            return new MvcHtmlString(string.Empty);
        }

        public static MvcHtmlString MyValidationMark(this HtmlHelper helper, string fieldName)
        {
            MvcHtmlString validationMsg = System.Web.Mvc.Html.ValidationExtensions.ValidationMessage(helper, fieldName);
            if (validationMsg != null)
            {
                return new MvcHtmlString("error");
            }

            return new MvcHtmlString(string.Empty);
        }

        public static Image TransformWithAspectRatio(Image image, int maxWidth, int maxHeight, bool thumbnail)
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
            string start, end;

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
                    Comments = album.Comments.Count(delegate (CommentModel comment){
                        return comment.Accepted??false;
                    }),
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

        public static string BaseURL()
        {
            string contentURI = new UrlHelper(HttpContext.Current.Request.RequestContext).Content("~");
            return string.Format("{0}://{1}{2}",
                HttpContext.Current.Request.Url.Scheme,
                HttpContext.Current.Request.Url.Authority,
                contentURI.Substring(0, contentURI.Length - 1));
        }
    }

    public enum ChartCategory { Popular, TopRated, Biggest, MostComments }
    public enum MainCategory { Home, Browse, Charts }

    public class MailSendJob
    {
        public string to { get; set; }
        public string body { get; set; }
        public string subject { get; set; }

        public void send()
        {
            Helpers.SendEmailContextFree(to, subject, body);
        }

    }
}
