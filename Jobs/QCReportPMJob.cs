using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp.Jobs
{
    public class QCReportPMJob : IJob
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Task Execute(IJobExecutionContext context)
        {

            log.Info(MYGlobal.getCurretnTime() + " -QCReportPMJob started \n");

            //Morning
            DateTime dt1 = DateTime.Now;
            //DateTime dt2 = dt1.AddDays(-1);
            //log.Info("Date yesterday " + dt2.ToString("yyyy-MM-dd"));
            warehouse.WHUpdator.doWarehouseQC(dt1, "AM");//Today morning done

            return Task.CompletedTask;
        }
    
}
}
