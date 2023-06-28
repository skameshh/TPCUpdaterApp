using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPC2UpdaterApp.Forms;

namespace TPC2UpdaterApp.Trigger
{
    class QCTrigger
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void doUpdateQC()
        {
            log.Info("QCTrigger doQCUpdate() start");
            QCHelper.doQCUpdate();
        }
    }
}
