using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PhotoHistory.Models;
using System.IO;
using System.Net;
using System.Drawing;

namespace PhotoHistory.Common
{
    public class FileHelper
    {
        private static readonly HttpServerUtility Server= HttpContext.Current.Server;
        public static readonly string UserRelativePath = "/Users";
        public static readonly string UsersDirectory = Server.MapPath("~" + UserRelativePath);
        private static readonly string UserSubdirectoryPrefix = "/user";
        private static readonly string AlbumSubdirectoryPrefix = "/album";
        private static readonly string PhotoPrefix = "photo";
       
        

        private static string UserPath(UserModel model,bool physical=true)
        {
                return physical? UsersDirectory + UserSubdirectoryPrefix + model.Id: UserSubdirectoryPrefix +model.Id;
        }

        public static string AlbumPath(AlbumModel model,bool physical=true)
        {
            return UserPath(model.User, physical) + AlbumSubdirectoryPrefix + model.Id+"/";
        }

        public static void CreateUserDirectory(UserModel model)
        {
            string path = UserPath(model);
            
            if( Directory.Exists(path) )
                System.Diagnostics.Debug.WriteLine(string.Format("Folder uzytkownika {0} istnieje", model.Id));
            else
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Tworze folder uzytkownika {0}", model.Id));
                Directory.CreateDirectory(path);
            }
            System.Diagnostics.Debug.WriteLine(string.Format("Sciezka {0}", path));

        }

        public static void CreateAlbumDirectory(AlbumModel model)
        {
            CreateUserDirectory(model.User);
            string path = AlbumPath(model);

            if (Directory.Exists(path))
                System.Diagnostics.Debug.WriteLine(string.Format("Folder albumu {0} istnieje", model.Id));
            else
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Tworze folder albumu  {0}", model.Id));
                Directory.CreateDirectory(path);
            }
            System.Diagnostics.Debug.WriteLine(string.Format("Sciezka {0}", path));
        }

        public static string SaveRemoteOrLocal(HttpPostedFileBase input, String remoteFilename, AlbumModel album)
        {
            if (input != null)
                return SavePhoto(input, album);
            else
                if (string.IsNullOrEmpty(remoteFilename))
                    throw new RemoteDownloadException("Wrong remote file name");
                else
                   return SavePhoto(remoteFilename, album);
        }

        public static string SavePhoto(HttpPostedFileBase input, AlbumModel album)
        {
            if (input.ContentType != "image/jpeg")
                throw new WrongPictureTypeException("Image is not an jpeg");
            return SaveFromStream(input.InputStream, album);

        }

        private static string SaveFromStream(Stream input, AlbumModel album)
        {
            CreateAlbumDirectory(album);
            Image img = Image.FromStream(input, true, true);

            if (!System.Drawing.Imaging.ImageFormat.Jpeg.Equals(img.RawFormat))
                throw new WrongPictureTypeException("Image is not an jpeg");
            string name =   "photo_" + DateTime.Now.ToString("yyyyMMddHHmmssff");
            Image thumbnail = img.GetThumbnailImage(100, 200, null, IntPtr.Zero);
            System.Diagnostics.Debug.WriteLine(AlbumPath(album) + name + "_mini.jpg");
            thumbnail.Save(AlbumPath(album)+name + "_mini.jpg");
            name += ".jpg";
            img.Save(AlbumPath(album)+name);
            return AlbumPath(album, false) + name;
        }

        public static string SavePhoto(String remoteFilename,AlbumModel album)
        {
            Stream remoteStream = null;
            Stream localStream = null;
            WebResponse response = null;
            try
            {
                WebRequest request = WebRequest.Create(remoteFilename);
                if (request != null)
                {
                    response = request.GetResponse();
                    if (response != null)
                    {
                        remoteStream = response.GetResponseStream();
                        return SaveFromStream(remoteStream, album);
                    }
                }
                else
                    throw new RemoteDownloadException("Can't download file from specified URL.");
            }
            catch (WrongPictureTypeException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new RemoteDownloadException("Can't download file from specified URL.", ex);
            }
                 
            finally
            {
                if (response != null) response.Close();
                if (remoteStream != null) remoteStream.Close();
                if (localStream != null) localStream.Close();
            }
            return "";
        }

        //Zwraca sciezki(wzgledne, nie fizyczne) do miniaturek 
        public static List<string> GetAlbumThumbnail(AlbumModel album)
        {
            List<string> list = new List<string>();
            return list;
        }
    }


    public class FileUploadException : Exception
    {
        public FileUploadException() : base() { }
        public FileUploadException(string message) : base(message) { }
        public FileUploadException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class WrongPictureTypeException : FileUploadException
    {
        public WrongPictureTypeException() : base() { }
        public WrongPictureTypeException(string message) : base(message) { }
        public WrongPictureTypeException(string message, Exception innerException) : base(message, innerException) { }

    }

    public class RemoteDownloadException : FileUploadException
    {
        public RemoteDownloadException() : base() { }
        public RemoteDownloadException(string message) : base(message) { }
        public RemoteDownloadException(string message, Exception innerException) : base(message, innerException) { }
    }
}