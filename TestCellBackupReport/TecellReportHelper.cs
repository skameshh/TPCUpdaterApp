using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * select * from [t_bup_everyday_files_count] where rdate>=DATEADD(day, -7, CAST(GETDATE() AS date)) and rdate<=DATEADD(day, 0, CAST(GETDATE() AS date))
order by rdate desc, testcell_name;

get last 1 week report and send it to the emails
 */
namespace TPC2UpdaterApp.TestCellBackupReport
{
    public class TecellReportHelper
    {

        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void doSendBackupEMail()
        {
            ArrayList al = doGetWeeklyData();

            string table_width = "65%";
            string aging_html = "";

            aging_html = aging_html + "<h5> <u> Testcell Backup </u> </h5>  ";
            aging_html = aging_html + "<table width=" + table_width + " align=\'center\' border=\'1\'>  <tr bgcolor=\"#009999\"> <td align=\'center\' colspan=\'6\'> Test Cell Backup </td> </tr>" +
                "<tr bgcolor=\"#FFFE33\"> <td    width=\'10%\' align=\'center\'> Test Cell </td> <td    width=\'10%\' align=\'center\'>Date  </td> <td   width=\'10%\'  align=\'center\'> Camera Count </td>    <td width=\'10%\' align=\'center\'  > Report Count </td>   <td width=\'10%\' align=\'center\'  > Canary Count </td>   <td width=\'10%\' align=\'center\'  > DB Count </td>   </tr> ";

            for (int x = 0; x < al.Count; x++)
            {
                TestCellBackupDao dao = (TestCellBackupDao)al[x];

                aging_html = aging_html + "<tr> <td align=\'center\'>" + dao.TestCellName + "</td>";
                aging_html = aging_html + "<td align=\'center\'>" + dao.Rdate.ToString("yyyy-MM-dd") + "</td>";
                aging_html = aging_html + "<td align=\'center\'>" + dao.CameraCount + "</td>";
                aging_html = aging_html + "<td align=\'center\'>" + dao.ReportCount + "</td>";
                aging_html = aging_html + "<td align=\'center\'>" + dao.CanaryCount + "</td>";
                aging_html = aging_html + "<td align=\'center\'>" + dao.DBCount + "</td> </tr>";

           }

            aging_html = aging_html + "</table><BR/>";

            string cc_emails = string.Empty;
            string to_email = string.Empty;

            cc_emails = cc_emails + "Alonso.Gebilaguin@halliburton.com,eugene.wye@halliburton.com, kamesh.shankaran@halliburton.com";
            to_email = to_email + "lydia.chen@halliburton.com";

            // to_email = "kamesh.shankaran@halliburton.com";

            MYGlobal.sendHALSMTPEmail("TestCellBackup@Halliburton.com", "All", "[TestcellBackup] Weekly testcell backup report : "+MYGlobal.getToday(), to_email, cc_emails, null, aging_html);
        }



        private static ArrayList doGetWeeklyData()
        {
            ArrayList al = new ArrayList();
            string sql = " select * from [t_bup_everyday_files_count] WITH (NOLOCK)  where rdate>=DATEADD(day, -7, CAST(GETDATE() AS date)) and rdate<=DATEADD(day, 0, CAST(GETDATE() AS date)) order by rdate desc, testcell_name";

            try
            {
                using (SqlConnection sqlCon = new SqlConnection(MYGlobal.getSing3MEIECString()))
                {
                    sqlCon.Open();
                    log.Info(" doGetWeeklyData()  sql = " + sql);
                    using (SqlCommand cmd = new SqlCommand(sql, sqlCon))
                    {
                        cmd.CommandType = CommandType.Text;
                        //cmd.Parameters.AddWithValue("@ACTION", action);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    TestCellBackupDao dao = new TestCellBackupDao();

                                    if ((reader["testcell_name"]) != DBNull.Value)
                                    {
                                        dao.TestCellName = ((string)reader["testcell_name"]);
                                    }
                                    dao.Rdate = ((DateTime)reader["rdate"]);
                                    dao.DBCount = ((int)reader["db_count"]);
                                    dao.CameraCount = ((int)reader["camera_count"]);
                                    dao.CanaryCount = ((int)reader["canary_count"]);
                                    dao.ReportCount = ((int)reader["report_count"]);


                                    al.Add(dao);

                                    log.Info("doGetWeeklyData Camera count=" + dao.CameraCount + ", dao.CanaryCount=" + dao.CanaryCount);
                                }
                                catch (Exception ee)
                                {
                                    log.Error("doGetWeeklyData Error = " + ee.Message);
                                }
                            }
                        }

                    }
                }
            }catch(Exception ee)
            {
                log.Error("EE " + ee.Message);
            }

            return al;
        }


    }
}
