using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TPC2UpdaterApp.Helpers;

namespace TPC2UpdaterApp.Jobs
{
    class TechExcelJob : IJob
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Task Execute(IJobExecutionContext context)
        {

            log.Info(MYGlobal.getCurretnTime() + " -TechExcelJob started \n");

            try
            {
                Thread.Sleep(5000);
                //working on it
                TechManagerExcel.doTechMgrExcel();
            }
            catch (Exception ee)
            {
                log.Error("Error doTechMgrExcel() " + ee.Message);
            }


            return Task.CompletedTask;
        }
    }
}
