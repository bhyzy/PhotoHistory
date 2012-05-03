using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PhotoHistory.Models;
using System.IO;
using System.Net;

namespace PhotoHistory.Common
{
    public class FileHelper
    {
        public static void createUserDirectory(UserModel model)
        {
        }

        public static void createAlbumDirectory(AlbumModel model)
        { 
        
        }

        public static void savePhoto(Stream input,AlbumModel album)
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
                        localStream = File.Create(localFilename);
                        byte[] buffer = new byte[1024];
                        int bytesRead;
                        do
                        {
                            bytesRead = remoteStream.Read(buffer, 0, buffer.Length);
                            localStream.Write(buffer, 0, bytesRead);
                            bytesProcessed += bytesRead;
                        } while (bytesRead > 0);
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