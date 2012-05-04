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
        public static readonly string rootDirectory = "~"; 
        
        public static void CreateUserDirectory(UserModel model)
        {
        }

        public static void CreateAlbumDirectory(AlbumModel model)
        { 
        
        }

        public static void SavePhoto(HttpPostedFileBase input, AlbumModel album)
        {

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

        public static int SavePhoto(String remoteFilename,AlbumModel album)
        {
            int bytesProcessed = 0;
            Stream remoteStream = null;
            Stream localStream = null;
            WebResponse response = null;
            String localFilename = null;

            try
            {
                WebRequest request = WebRequest.Create(remoteFilename);
                
                if (request != null)
                {
                    response = request.GetResponse();
                    if (response != null)
                    {
                        remoteStream = response.GetResponseStream();
                        Image img=Image.FromStream(remoteStream, true, true);
                        if (System.Drawing.Imaging.ImageFormat.Jpeg.Equals(img.RawFormat))
                            throw new WrongPictureTypeException("Image is not an jpeg");

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (response != null) response.Close();
                if (remoteStream != null) remoteStream.Close();
                if (localStream != null) localStream.Close();
            }
            return bytesProcessed;
        }

        public static string GetPhoto()
        {
            return null;
        }

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