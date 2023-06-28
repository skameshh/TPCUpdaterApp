using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPC2UpdaterApp.Forms;
using TPC2UpdaterApp.Helpers;
using TPC2UpdaterApp.ThreadProtector;

namespace TPC2UpdaterApp.Trigger
{
    class PlannerTrigger
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void doPlanner()
        {

            try { 
            // Update completed zewo from sap
              //  ZEWOStatusUpdate4Planner.doUpdateZewostatus();
            }
            catch (Exception ee)
            {
                log.Error(ee.Message);
            }

            try
            {
                //update
                log.Info("doPlanner() start a planner");
                PlannerImport pi = new PlannerImport();
                pi.Show();
            }catch(Exception ee)
            {
               log.Error(  ee.Message );
            }

           

        }
    }
}
