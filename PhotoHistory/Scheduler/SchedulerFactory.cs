using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using Quartz.Impl;

namespace PhotoHistory.Scheduler
{
    public static class SchedulerFactory
    {
        private static IScheduler Scheduler {get; set; }

        public static void InitScheduler(){
          ISchedulerFactory schedFact = new StdSchedulerFactory();
            Scheduler = schedFact.GetScheduler();
            Scheduler.Start();
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