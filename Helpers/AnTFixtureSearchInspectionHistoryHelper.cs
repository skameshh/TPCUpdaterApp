using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPC2UpdaterApp.DB;

namespace TPC2UpdaterApp.Helpers
{
    public class AnTFixtureSearchInspectionHistoryHelper
    {

        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void doSendFixtureInspEMail()
        {
            ArrayList gc_al = doGetMonthlyData("GC");
            ArrayList ici_al = doGetMonthlyData("ICI");

            string table_width = "80%";
            string aging_html = "";
            string html1 = string.Empty;
            string html2 = string.Empty;

            aging_html = aging_html + getGCTemplate(table_width, html1, gc_al, "GC");
            aging_html = aging_html + getGCTemplate(table_width, html2, ici_al, "ICI");

            string cc_emails = string.Empty;
            string to_email = string.Empty;

            cc_emails = cc_emails + "Alonso.Gebilaguin@halliburton.com, eugene.wye@halliburton.com, kamesh.shankaran@halliburton.com";
            to_email = to_email + "SengWhatt.Ng@Halliburton.com, Jeff.Huen@halliburton.com, Siva.Elumalai@halliburton.com,  lydia.chen@halliburton.com";


           //  to_email = "kamesh.shankaran@halliburton.com";
           // cc_emails = to_email;

            MYGlobal.sendHALSMTPEmail("AnTFixtureInspection@Halliburton.com", "All", "[AnTFixtureInspection] Monthly report : " + MYGlobal.getToday(), to_email, cc_emails, null, aging_html);
        }



        private static string getGCTemplate(string table_width, string aging_html, ArrayList gc_al, string bunit)
        {
            // === GC =======
            aging_html = aging_html + "<h5> <u> "+ bunit + " Fixture Inspection due for this month " + gc_al.Count + " Nos </u> </h5>  ";
            aging_html = aging_html + "<table width=" + table_width + " align=\'center\' border=\'1\'>  <tr bgcolor=\"#009999\"> <td align=\'center\' colspan=\'7\'> Fixture Inspection due for this month</td> </tr>" +
                "<tr bgcolor=\"#FFFE33\">  <td  width=\'5%\' align=\'center\'> Business Unit </td> <td width=\'12%\' align=\'center\'> Fixture </td> " +
                "<td width=\'10%\' align=\'center\'>Last Inspection Date  </td> <td width=\'10%\'  align=\'center\'> Next Inspection Date </td>   " +
                " <td width=\'10%\' align=\'center\'> Remarks </td>   <td width=\'10%\' align=\'center\'  > Usage Count </td>   <td width=\'10%\' align=\'center\'  > Location </td>    </tr> ";


            for (int x = 0; x < gc_al.Count; x++)
            {
                AnTInspectionHistoryDao dao = (AnTInspectionHistoryDao)gc_al[x];

                aging_html = aging_html + "<tr> <td align=\'center\'>" + dao.BusinessUnit + "</td>";
                aging_html = aging_html + "  <td align=\'left\'>" + dao.FixtureId + "</td>";
                aging_html = aging_html + "<td align=\'center\'>" + dao.LastInsDate.ToString("yyyy-MM-dd") + "</td>";
                aging_html = aging_html + "<td align=\'center\'>" + dao.NextInsDate.ToString("yyyy-MM-dd") + "</td>";
                aging_html = aging_html + "<td align=\'center\'>" + dao.Remarks + "</td>";
                aging_html = aging_html + "<td align=\'center\'>" + dao.UsageCount + "</td>";
                aging_html = aging_html + "<td align=\'left\'>" + dao.LocationQR + "</td>";
                aging_html = aging_html + "</tr>";
            }

            aging_html = aging_html + "<tr><td align=\'right\' colspan=\'6\'> Total Fixtures </td> <td align=\'center\'>" + gc_al.Count + " </td> ";
            aging_html = aging_html + "</tr>";
            aging_html = aging_html + "</table><BR/>";

            return aging_html;
        }


        private static ArrayList doGetMonthlyData(string bunit)
        {
            ArrayList al = new ArrayList();
            string sql = " select * from [view_material_fixture_inspection] with (nolock) where business_unit='" + bunit + "' and year(next_inspection_date) = year(GETDATE()) " +
                " and month(next_inspection_date) = month(GETDATE()) order by next_inspection_date;";

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
                                    AnTInspectionHistoryDao dao = new AnTInspectionHistoryDao();

                                    if ((reader["fixture_qr_id"]) != DBNull.Value)
                                    {
                                        dao.FixtureId = ((string)reader["fixture_qr_id"]);
                                    }
                                    dao.LastInsDate = ((DateTime)reader["last_inspection_date"]);
                                    dao.NextInsDate = ((DateTime)reader["next_inspection_date"]);

                                    dao.Status = ((int)reader["status"]);

                                    if ((reader["remarks"]) != DBNull.Value)
                                    {
                                        dao.Remarks = ((string)reader["remarks"]);
                                    }
                                    dao.BusinessUnit = ((string)reader["business_unit"]);
                                    dao.LocationQR = ((string)reader["location_qr"]);
                                                                                           
                                    dao.UsageCount = ((int)reader["usuage_count"]);


                                    al.Add(dao);

                                    log.Info("doGetMonthlyData fixture_id count=" + dao.FixtureId + ", dao.NextInsDate =" + dao.NextInsDate);
                                }
                                catch (Exception ee)
                                {
                                    log.Error("doGetMonthlyData Error = " + ee.Message);
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ee)
            {
                log.Error("doGetMonthlyData EE " + ee.Message);
            }

            return al;
        }


    }
}
