using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPC2UpdaterApp.Trigger;

namespace TPC2UpdaterApp.Jobs
{
    class FOJob : IJob
    {

        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public Task Execute(IJobExecutionContext context)
        {
            // throw new NotImplementedException

            log.Info(MYGlobal.getCurretnTime() + " -ME2NJob started ");

            FOTrigger.doFOUpdate();

            return Task.CompletedTask;

        }
    }
}
