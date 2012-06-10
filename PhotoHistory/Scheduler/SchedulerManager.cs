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
        private static IScheduler Scheduler {get; set; }
        private static String rootPath;



        public static void InitScheduler(String link){
            if (rootPath != null)
                return;
            rootPath = link;
          ISchedulerFactory schedFact = new StdSchedulerFactory();
            Scheduler = schedFact.GetScheduler();
            Scheduler.Start();
            IJobDetail jobDetail = JobBuilder.Create<PhotoNotifyJob>().WithIdentity("PhotoNotificationJob").Build();
            jobDetail.JobDataMap.Add("path", rootPath);
            ITrigger trigger2 = (ISimpleTrigger)TriggerBuilder.Create()
                                           .WithIdentity("trigger")
                                           .StartAt(DateTime.Now.AddSeconds(10)) // wystartuj scheduler 10s
                                           //.WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever())
                                           .WithSimpleSchedule(x => x.WithIntervalInHours(1).RepeatForever()) //dla testow interwal 1h, ma byc 24
                                           .Build();
            DateTimeOffset ft = Scheduler.ScheduleJob(jobDetail, trigger2);
            System.Diagnostics.Debug.WriteLine(jobDetail.Key + " has been scheduled to run at: " + ft);
        }

        public static IScheduler getScheduler(){
            return Scheduler;
        }

    }
}