using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using PhotoHistory.Models;
using PhotoHistory.Data;
using System.Security.Policy;

namespace PhotoHistory.Scheduler
{
    public class PhotoNotifyJob : IJob
    {
        public PhotoNotifyJob() { }

        public void Execute(IJobExecutionContext context)
        {
            System.Diagnostics.Debug.WriteLine("Executing photo notification job");
            AlbumRepository repo = new AlbumRepository();
            List<AlbumModel> albums = repo.GetAll();
            foreach(AlbumModel album in albums)
            {
                if (album.User.NotifyPhoto)
                {
                    DateTime time = album.NextNotification ?? DateTime.Today;
                    time = time.Date.AddDays(album.NotificationPeriod??1);
                    if (time <= DateTime.Today)
                    {
                        album.NextNotification = DateTime.Today.AddDays(album.NotificationPeriod ?? 1);
                        repo.Update(album);
                        string link = new Url("~").ToString();
                        string body = string.Format("Hello {0},</br>It's time to add new photo to your album <a href='{1}'>{2}</a>",
                            album.User.Login,link,album.Name);
                        System.Diagnostics.Debug.WriteLine(body);
                        //Helpers.SendEmail(album.User.Email, "Photo notification", body);
                    }
                
                }
            }
        }
    }


}