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
        private static readonly HttpServerUtility Server = HttpContext.Current.Server;
        public static readonly string UserRelativePath = "/Users";
        public static readonly string UsersDirectory = Server.MapPath("~" + UserRelativePath);
        private static readonly string UserSubdirectoryPrefix = "/user";
        private static readonly string AlbumSubdirectoryPrefix = "/album";

        public static readonly ushort MAX_WIDTH = 1024;
        public static readonly ushort MAX_HEIGHT = 800;
        public static readonly ushort THUMB_WIDTH = 133;
        public static readonly ushort THUMB_HEIGHT = 100;

        private static string UserPath(UserModel model, bool physical = true)
        {
            return physical ? UsersDirectory + UserSubdirectoryPrefix + model.Id : UserRelativePath + UserSubdirectoryPrefix + model.Id;
        }

        public static string AlbumPath(AlbumModel model, bool physical = true)
        {
            return UserPath(model.User, physical) + AlbumSubdirectoryPrefix + model.Id + "/";
        }

        public static string getPhotoPath(AlbumModel album, string photoName)
        {
            return AlbumPath(album, false) + photoName;
        }

        public static void CreateUserDirectory(UserModel model)
        {
            string path = UserPath(model);

            if (Directory.Exists(path))
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

        public static string SaveRemoteOrLocal(HttpPostedFileBase input, String remoteFilename, AlbumModel album, string name)
        {
            if (input != null)
                return SavePhoto(input, album, name);
            else
                if (string.IsNullOrEmpty(remoteFilename))
                    throw new FileUploadException("Can't upload your photo from provided URL. Please check your URL and try again later.");
                else
                    return SavePhoto(remoteFilename, album, name);
        }

        public static string SavePhoto(HttpPostedFileBase input, AlbumModel album, string name)
        {
            if (input.ContentType != "image/jpeg")
                throw new FileUploadException("You must upload jpeg image.");
            return SaveFromStream(input.InputStream, album, name);

        }

        private static string SaveFromStream(Stream input, AlbumModel album, string name)
        {
            CreateAlbumDirectory(album);
            Image img = Image.FromStream(input, true, true);

            if (!System.Drawing.Imaging.ImageFormat.Jpeg.Equals(img.RawFormat))
                throw new FileUploadException("You must upload jpeg image.");

            Image thumbnail = img.GetThumbnailImage(THUMB_WIDTH, THUMB_HEIGHT, null, IntPtr.Zero);

            System.Diagnostics.Debug.WriteLine(AlbumPath(album) + name + "_mini.jpg");

            thumbnail.Save(AlbumPath(album) + name + "_mini.jpg");
            name += ".jpg";
            img.Save(AlbumPath(album) + name);
            return AlbumPath(album, false) + name;
        }

        public static string SavePhoto(String remoteFilename, AlbumModel album,string name)
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
                        return SaveFromStream(remoteStream, album, name);
                    }
                }
                else
                    throw new FileUploadException("Can't upload your photo from provided URL. Please check your URL and try again later.");
            }
            catch (FileUploadException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new FileUploadException("Can't download file from specified URL.", ex);
            }

            finally
            {
                if (response != null) response.Close();
                if (remoteStream != null) remoteStream.Close();
                if (localStream != null) localStream.Close();
            }
            return "";
        }

        public static void GetDate(AlbumModel album, out string start, out string end)
        {
            DirectoryInfo dir = new DirectoryInfo(AlbumPath(album));

            if (!dir.Exists)
            {
                CreateAlbumDirectory(album);
            }

            FileSystemInfo[] files = dir.GetFileSystemInfos("*_mini.jpg");
            Array.Sort<FileSystemInfo>(files, delegate(FileSystemInfo a, FileSystemInfo b)
            {
                return a.CreationTime.CompareTo(b.CreationTime);
            });

            if (files.Length == 0)
            {
                start = "";
                end = "";
                return;
            }
            start = files.First().CreationTime.ToString("dd/MM/yyyy");
            end = files.Last().CreationTime.ToString("dd/MM/yyyy");
        }

        //Zwraca sciezki(wzgledne, nie fizyczne) do miniaturek 
        public static List<string> GetAlbumThumbnail(AlbumModel album)
        {
            DirectoryInfo dir = new DirectoryInfo(AlbumPath(album));
            FileSystemInfo[] files = dir.GetFileSystemInfos("*_mini.jpg");
            Array.Sort<FileSystemInfo>(files, delegate(FileSystemInfo a, FileSystemInfo b)
            {
                return a.CreationTime.CompareTo(b.CreationTime);
            });

            List<string> list = new List<string>();
            foreach (FileSystemInfo file in files)
            {
                list.Add(AlbumPath(album, false) + file.Name);
            }
            return list;
        }
    }
}

 public class FileUploadException : Exception
 {
     public FileUploadException() : base() { }
     public FileUploadException(string message) : base(message) { }
     public FileUploadException(string message, Exception innerException) : base(message, innerException) { }
 }
