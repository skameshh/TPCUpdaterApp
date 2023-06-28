using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TPC2UpdaterApp.Forms;
using TPC2UpdaterApp.Helpers;

namespace TPC2UpdaterApp.Trigger
{
    class BackupDBTrigger
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void doBackupDB()
        {
            log.Info("doBackupDB() start");
            //====== TPC =====
            string tpc_backupFolder = ConfigurationManager.AppSettings["BackupFolder_TPC"];
            BackupDBHelper.doBackup(tpc_backupFolder, MYGlobal.getCString());

            Thread.Sleep(5000);
            //======== RFQ ========
            string rfq_backupFolder = ConfigurationManager.AppSettings["BackupFolder_RFQ"];
            BackupDBHelper.doBackup(rfq_backupFolder, MYGlobal.getSing3HALRFQ());
            Thread.Sleep(5000);


            //======== MEIE ========
            string meie_backupFolder = ConfigurationManager.AppSettings["BackupFolder_MEIE"];
            BackupDBHelper.doBackup(meie_backupFolder, MYGlobal.getSing3MEIECString());
            Thread.Sleep(5000);
          
            //======== LOTOLOCK ========
            string loto_backupFolder = ConfigurationManager.AppSettings["BackupFolder_LOTO"];
            BackupDBHelper.doBackup(loto_backupFolder, MYGlobal.getSing3HALMAINLotoLock());
            Thread.Sleep(5000);


            //======== TECHDB ========
            string techdb_backupFolder = ConfigurationManager.AppSettings["BackupFolder_TECHDB"];
            BackupDBHelper.doBackup(techdb_backupFolder, MYGlobal.getTechDBCString());
            Thread.Sleep(5000);


            //======== TECHDB ========
            DriftGageTrackSync.doAllGageTrackUpdate();
        }
    }
}
