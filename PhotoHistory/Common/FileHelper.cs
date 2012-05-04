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
        public static readonly HttpServerUtility Server= HttpContext.Current.Server;
        public static readonly string UsersDirectory = Server.MapPath("~/Users");

        public static string UserPhysicalPath(UserModel model)
        {
            return UsersDirectory + "/user" + model.Id + "/";
        }

        public static string AlbumPhysicalPath(AlbumModel model)
        {
            return UserPhysicalPath(model.User) + "album" + model.Id + "/";
        }

        public static void CreateUserDirectory(UserModel model)
        {
            string path = UserPhysicalPath(model);
            
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
            string path = AlbumPhysicalPath(model);

            if (Directory.Exists(path))
                System.Diagnostics.Debug.WriteLine(string.Format("Folder albumu {0} istnieje", model.Id));
            else
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Tworze folder albumu  {0}", model.Id));
                Directory.CreateDirectory(path);
            }
            System.Diagnostics.Debug.WriteLine(string.Format("Sciezka {0}", path));
        }

        public static void SaveRemoteOrLocal(HttpPostedFileBase input, String remoteFilename, AlbumModel album)
        {
            if (input != null)
                SavePhoto(input, album);
            else
                if (string.IsNullOrEmpty(remoteFilename))
                    throw new FileUploadException("Wrong remote file name");
                else
                    SavePhoto(remoteFilename, album);
        }

        public static void SavePhoto(HttpPostedFileBase input, AlbumModel album)
        {
            if (input.ContentType != "image/jpeg")
                throw new WrongPictureTypeException("Image is not an jpeg");
            CreateAlbumDirectory(album);
            SaveFromStream(input.InputStream, album);

        }

        private static void SaveFromStream(Stream input, AlbumModel album)
        {
            Image img = Image.FromStream(input, true, true);
            if (System.Drawing.Imaging.ImageFormat.Jpeg.Equals(img.RawFormat))
                throw new WrongPictureTypeException("Image is not an jpeg");
        }



        public static int SavePhoto(String remoteFilename,AlbumModel album)
        {
            int bytesProcessed = 0;
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
                        SaveFromStream(remoteStream, album);
                    }
                }
                else
                    throw new RemoteDownloadException("Can't download file from specified URL.");
            }
            finally
            {
                if (response != null) response.Close();
                if (remoteStream != null) remoteStream.Close();
                if (localStream != null) localStream.Close();
            }
            return bytesProcessed;
        }

        //Zwraca 3 miniaturki (sciezki ktore mozna umiescic w <img src=...>), poczatek, srodek, koniec
        public static IEnumerable<string> GetAlbumThumbail()
        {
            return null;
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