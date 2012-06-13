using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using Quartz.Impl;
using PhotoHistory.Models;
using PhotoHistory.Data;
using System.Web.Routing;

namespace PhotoHistory.Scheduler
{
    public static class SchedulerManager
    {
        private static IScheduler Scheduler { get; set; }
        private static String rootPath;

        public static void InitScheduler(String link)
        {
            if (rootPath != null)
                return;
            rootPath = link;
            ISchedulerFactory schedFact = new StdSchedulerFactory();
            Scheduler = schedFact.GetScheduler();
            Scheduler.Start();
            IJobDetail jobDetail = JobBuilder.Create<PhotoNotifyJob>().WithIdentity("PhotoNotificationJob").Build();
            jobDetail.JobDataMap.Add("path", rootPath);
            //Wystartuj jak najwczesniej, o godzinie 2:00 rano
            DateTime startAt = DateTime.Today;

            startAt = startAt.AddHours(2);
            if (startAt < DateTime.Now) //jesli juz minela godzina 2, wystartuj nastepnego dnia
                startAt = startAt.AddDays(1);

            ITrigger trigger2 = (ISimpleTrigger)TriggerBuilder.Create()
                                           .WithIdentity("trigger")
                                           .StartAt(startAt) // wystartuj scheduler o zadanej godzinie
                                           .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever()) //odpalaj notyfikacje co 24h, bez limitu powtorzen
                                           .Build();
            DateTimeOffset ft = Scheduler.ScheduleJob(jobDetail, trigger2);
            System.Diagnostics.Debug.WriteLine(jobDetail.Key + " has been scheduled to run at: " + ft);
        }

        public static IScheduler getScheduler()
        {
            return Scheduler;
        }

    }
}