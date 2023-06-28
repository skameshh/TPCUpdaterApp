using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPC2UpdaterApp.TestCellBackupReport;

namespace TPC2UpdaterApp.Trigger
{
    public class TestCellBackupTrigger
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void doTestcellbackup()
        {
            TecellReportHelper.doSendBackupEMail();
        }
    }
}
