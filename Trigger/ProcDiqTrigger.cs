using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPC2UpdaterApp.ProcurementDIQ;
using TPC2UpdaterApp.ThreadProtector;

namespace TPC2UpdaterApp.Trigger
{
    public class ProcDiqTrigger
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void doProcUpdate()
        {
            log.Info("ProcurementDIQUpdator.doProcrUpdate() \n\n");
            ProcurementDIQUpdator.doProcrUpdate();

            //========= ThreadProtectorUpdate ========
           /* try
            {
                log.Info("ThreadProtectorUpdate \n\n");
                //ThreadProtectorUpdate.doThreadProtectorUpdate();
            }
            catch (Exception ee)
            {
                log.Error("Error in : " + ee.Message);
            }*/


        }

    }
}
