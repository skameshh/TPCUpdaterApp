using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPC2UpdaterApp.Forms;
using TPC2UpdaterApp.ProcurementDIQ;

/**
 * 
 * 
 * GlobalReportJob
 **/
namespace TPC2UpdaterApp.Trigger
{
    public class GlobalReportTrigger
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void doGlobalReport()
        {
            log.Info("doGlobalReport() start");
            GlobalReportHelper.doGlobalReport();

            //log.Info("ProcurementDIQUpdator.doProcrUpdate() \n\n");
           // ProcurementDIQUpdator.doProcrUpdate();
        }
    }
}
