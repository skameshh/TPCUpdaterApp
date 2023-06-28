using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp.Forms
{
    class BackupDBHelper
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public static void doBackup(string backupFolder, string cstring)
        {
            try
            {
                //var backupFolder = ConfigurationManager.AppSettings["BackupFolder_TPC"];
                var sqlConStrBuilder = new SqlConnectionStringBuilder(cstring);
                var backupFileName = String.Format("{0}{1}-{2}.bak",
                        backupFolder, sqlConStrBuilder.InitialCatalog,
                        DateTime.Now.ToString("yyyy-MM-dd"));
                log.Info("backupFileName  = " + backupFileName);

                using (SqlConnection cnn = new SqlConnection(cstring))
                {
                    var query = String.Format("BACKUP DATABASE {0} TO DISK='{1}'",
                    sqlConStrBuilder.InitialCatalog, backupFileName);
                    log.Info("Backup query = " + query);

                    using (var command = new SqlCommand(query, cnn))
                    {
                        try
                        {
                            cnn.Open();
                            command.ExecuteNonQuery();
                            log.Info("Backup completed successfully \n\n ");
                        }
                        catch (Exception ee)
                        {
                            log.Info("doBackup() Error in backup " + ee.Message);
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                log.Error(" doBackup() " + ee.Message);
            }
        }



        public static void doRFQBackup(string backupFolder)
        {
            try
            {
                //var backupFolder = ConfigurationManager.AppSettings["BackupFolder_TPC"];
                var sqlConStrBuilder = new SqlConnectionStringBuilder(MYGlobal.getSing3HALRFQ());
                var backupFileName = String.Format("{0}{1}-{2}.bak",
                        backupFolder, sqlConStrBuilder.InitialCatalog,
                        DateTime.Now.ToString("yyyy-MM-dd"));
                log.Info("backupFileName  = " + backupFileName);

                using (SqlConnection cnn = new SqlConnection(MYGlobal.getSing3HALRFQ()))
                {
                    var query = String.Format("BACKUP DATABASE {0} TO DISK='{1}'",
                    sqlConStrBuilder.InitialCatalog, backupFileName);
                    log.Info("Backup query = " + query);

                    using (var command = new SqlCommand(query, cnn))
                    {
                        try
                        {
                            cnn.Open();
                            command.ExecuteNonQuery();
                            log.Info("doRFQBackup() Backup completed successfully \n\n ");
                        }
                        catch (Exception ee)
                        {
                            log.Info("doRFQBackup() Error in backup " + ee.Message);
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                log.Error(" doRFQBackup() " + ee.Message);
            }
        }


    }
}
