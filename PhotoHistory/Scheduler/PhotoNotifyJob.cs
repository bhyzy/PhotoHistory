using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using PhotoHistory.Models;
using PhotoHistory.Data;
using System.Security.Policy;
using System.Net.Mail;
using System.Net;


namespace PhotoHistory.Scheduler
{
    public class PhotoNotifyJob : IJob
    {
        public PhotoNotifyJob() { }
        private int unit = 1;

        private void SendEmail(String to, String subject,String body)
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

        public void Execute(IJobExecutionContext context)
        {
            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString()+" Executing photo notification job");
            AlbumRepository repo = new AlbumRepository();
            List<AlbumModel> albums = repo.GetAll();

            string path = context.JobDetail.JobDataMap.GetString("path");
            foreach(AlbumModel album in albums)
            {
                if (album.User.NotifyPhoto && album.NotificationPeriod!=null)
                {
                    DateTime time = album.NextNotification ?? DateTime.Today;
                    time = time.Date; //Data, godziny niepotrzebne
                    if (time <= DateTime.Today)
                    //if (time <= DateTime.Now)
                    {
                        album.NextNotification = DateTime.Today.AddDays(album.NotificationPeriod ?? 1);
                        //album.NextNotification = time.AddMinutes(album.NotificationPeriod * unit ?? 1);
                        repo.Update(album);

                        string link = path + "/Album/AddPhoto?albumId="+album.Id;

                        string body = string.Format("Hello {0},</br>It's time to add new photo to your album <a href='{1}'>{2}</a>",
                            album.User.Login,link,album.Name);
                        SendEmail(album.User.Email, "Photo notification", body);
                        System.Diagnostics.Debug.WriteLine("Sending email");
                    }
                
                }
            }
        }
    }


}