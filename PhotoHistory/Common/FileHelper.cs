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
    public class WrongPictureTypeException : Exception
    {
        WrongPictureTypeException():base(){}
        WrongPictureTypeException(string message) : base(message) { }
        WrongPictureTypeException(string message, Exception innerException) : base(message, innerException) { }
        
    }

    public class RemoteDownloadException : Exception
    {
        RemoteDownloadException() : base() { }
        RemoteDownloadException(string message) : base(message) { }
        RemoteDownloadException(string message, Exception innerException) : base(message, innerException) { }

    }

    public class FileHelper
    {
        public static void createUserDirectory(UserModel model)
        {
        }

        public static void createAlbumDirectory(AlbumModel model)
        { 
        
        }

        public static void savePhoto(HttpPostedFileBase input, AlbumModel album)
        {

        }

        public static int DownloadFile(String remoteFilename,AlbumModel album)
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
                            throw new Exception("Image is not jpeg");

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

        public static string getPhoto()
        {
            return null;
        }

        public static IEnumerable<string> getAlbumThumbail()
        {
            return null;
        }
    }
}