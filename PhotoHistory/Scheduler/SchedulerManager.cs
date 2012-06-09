using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using Quartz.Impl;
using PhotoHistory.Models;
using PhotoHistory.Data;

namespace PhotoHistory.Scheduler
{
    public static class SchedulerManager
    {
        private static IScheduler Scheduler {get; set; }

        public static void InitScheduler(){
          ISchedulerFactory schedFact = new StdSchedulerFactory();
            Scheduler = schedFact.GetScheduler();
            Scheduler.Start();
            IJobDetail jobDetail = JobBuilder.Create<PhotoNotifyJob>().WithIdentity("PhotoNotificationJob").Build();
            ICronTrigger trigger = (ICronTrigger)TriggerBuilder.Create()
                                                      .WithIdentity("trigger1")
                                                      .WithCronSchedule("0/20 * * * * ?")
                                                      .Build();
            ITrigger trigger2 = (ISimpleTrigger)TriggerBuilder.Create()
                                           .WithIdentity("trigger6", "group1")
                                           .StartAt(DateTime.Now.AddSeconds(20))
                                           .WithSimpleSchedule(x => x.WithIntervalInSeconds(40).RepeatForever())
                                           .Build();
            DateTimeOffset ft = Scheduler.ScheduleJob(jobDetail, trigger2);
            System.Diagnostics.Debug.WriteLine(jobDetail.Key + " has been scheduled to run at: " + ft);
        }

        public static IScheduler getScheduler(){
            return Scheduler;
        }

  

        //// construct job info
        //JobDetail jobDetail = new JobDetail("myJob", null, typeof(HelloJob));
        //// fire every hour
        //Trigger trigger = TriggerUtils.MakeHourlyTrigger();
        //// start on the next even hour
        //trigger.StartTimeUtc = TriggerUtils.GetEvenHourDate(DateTime.UtcNow);  
        //trigger.Name = "myTrigger";
        //sched.ScheduleJob(jobDetail, trigger); 
    }
}