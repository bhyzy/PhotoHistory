using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;

namespace PhotoHistory.Scheduler
{
    public class PhotoNotifyJob : IJob
    {
        public PhotoNotifyJob() { }

        public void Execute(IJobExecutionContext context)
        {
            System.Diagnostics.Debug.WriteLine("Executing");
        }
    }


}