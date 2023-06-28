using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * Attached to global trigger at 6am 
 * GlobalReportTrigger
 * To run this there must be another one to update t2_proc_operations
 * ======= TurnkeyFarmoutHelper ======= must be executed
 */
namespace TPC2UpdaterApp.ProcurementDIQ
{
   

    public class ProcurementDIQUpdator
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ProcurementDIQUpdator()
        {
            //using cardev DB
        }


        public static void doProcrUpdate()
        {
            ArrayList al = doGetAllProc();
           

            if (al.Count == 0)
            {
                //No need to send any email.
                return;
            }
            else
            {
               
            }                     


            int so_aging_lessoreq_3 = 0;
            int so_aging_4to10 = 0;
            int so_aging_gt_10 = 0;
            String  rem_lt_3_days = string.Empty;
            string rem_4to10_days = string.Empty;
            string rem_gt10_days = string.Empty;
            string table_width = "65%";

            //===================== SO AGING ===============

            string aging_html = "";

            ArrayList soList = doGetSOTKFOList(2); ;
            aging_html = aging_html + "<h5> <u> TPC Project Aging  new TPC Parts </u> </h5>  ";
            aging_html = aging_html + "<table width="+ table_width + " align=\'center\' border=\'1\'>  <tr bgcolor=\"#009999\"> <td align=\'center\' colspan=\'4\'> SO Aging </td> </tr>" +
                "<tr bgcolor=\"#FFFE33\"> <td    width=\'10%\' align=\'center\'> Material# </td> <td    width=\'10%\' align=\'center\'>PO#  </td> <td   width=\'20%\'  align=\'center\'> Remarks </td>    <td width=\'5%\' align=\'center\'  > DIQ </td>   </tr> ";
            
            for(int x = 0; x < soList.Count; x++)
            {
                ProcAgingDao sodao = (ProcAgingDao)soList[x];

                aging_html = aging_html + "<tr> <td align=\'center\'>" + sodao.MaterialNum + "</td>";
                aging_html = aging_html + "<td align=\'center\'>" + sodao.ProdOrdNum + "</td>";
                aging_html = aging_html + "<td align=\'left\'>" + sodao.Remarks + "</td>";
                aging_html = aging_html + "<td align=\'center\'>" + sodao.Diq + "</td> </tr>";

                if (sodao.Diq <= 3)
                {
                    so_aging_lessoreq_3 = so_aging_lessoreq_3 + 1;
                    rem_lt_3_days = rem_lt_3_days + sodao.Remarks +" ";
                }
                else if(sodao.Diq>3 && sodao.Diq < 11){
                    so_aging_4to10 = so_aging_4to10 + 1;
                    rem_4to10_days = rem_4to10_days + sodao.Remarks + " ";
                }
                else if (sodao.Diq>10)
                {
                    so_aging_gt_10 = so_aging_gt_10 + 1;
                    rem_gt10_days = rem_gt10_days + sodao.Remarks + " ";
                }
            }


            aging_html = aging_html + "</table><BR/>";

            //===================== TK AGING ===============

            int tk_aging_lessoreq_3 = 0;
            int tk_aging_4to10 = 0;
            int tk_aging_gt_10 = 0;

            ArrayList tkList = doGetSOTKFOList(3);
            aging_html = aging_html + "<table width=" + table_width + " align=\'center\' border=\'1\'>  <tr bgcolor=\"#009999\"> <td align=\'center\' colspan=\'4\'> TK Aging </td> </tr>" +
              "<tr bgcolor=\"#FFFE33\"> <td    width=\'10%\' align=\'center\'> Material# </td> <td    width=\'10%\' align=\'center\'>PO#  </td> <td   width=\'20%\'  align=\'center\'> Remarks </td>    <td width=\'5%\' align=\'center\'  > DIQ </td>   </tr> ";

            for (int x = 0; x < tkList.Count; x++)
            {
                ProcAgingDao sodao = (ProcAgingDao)tkList[x];

                aging_html = aging_html + "<tr> <td align=\'center\'>" + sodao.MaterialNum + "</td>";
                aging_html = aging_html + "<td align=\'center\'>" + sodao.ProdOrdNum + "</td>";
                aging_html = aging_html + "<td align=\'left\'>" + sodao.Remarks + "</td>";
                aging_html = aging_html + "<td align=\'center\'>" + sodao.Diq + "</td> </tr>";


                if (sodao.Diq <= 3)
                {
                    tk_aging_lessoreq_3 = tk_aging_lessoreq_3 + 1;
                    rem_lt_3_days = rem_lt_3_days + sodao.Remarks + " ";
                }
                else if (sodao.Diq > 3 && sodao.Diq < 11)
                {
                    tk_aging_4to10 = tk_aging_4to10 + 1;
                    rem_4to10_days = rem_4to10_days + sodao.Remarks + " ";
                }
                else if (sodao.Diq > 10)
                {
                    tk_aging_gt_10 = tk_aging_gt_10 + 1;
                    rem_gt10_days = rem_gt10_days + sodao.Remarks + " ";
                }


            }


            aging_html = aging_html + "</table><BR/>";



            //===================== FO AGING ===============

            int fo_aging_lessoreq_3 = 0;
            int fo_aging_4to10 = 0;
            int fo_aging_gt_10 = 0;


            ArrayList foList = doGetSOTKFOList(4);
            aging_html = aging_html + "<table width=" + table_width + " align=\'center\' border=\'1\'>  <tr bgcolor=\"#009999\"> <td align=\'center\' colspan=\'4\'> FO Aging </td> </tr>" +
                "<tr bgcolor=\"#FFFE33\"> <td    width=\'10%\' align=\'center\'> Material# </td> <td    width=\'10%\' align=\'center\'>PO#  </td> <td   width=\'20%\'  align=\'center\'> Remarks </td>    <td width=\'5%\' align=\'center\'  > DIQ </td>   </tr> ";

            for (int x = 0; x < foList.Count; x++)
            {
                ProcAgingDao sodao = (ProcAgingDao)foList[x];

                aging_html = aging_html + "<tr> <td align=\'center\'>" + sodao.MaterialNum + "</td>";
                aging_html = aging_html + "<td align=\'center\'>" + sodao.ProdOrdNum + "</td>";
                aging_html = aging_html + "<td align=\'left\'>" + sodao.Remarks + "</td>";
                aging_html = aging_html + "<td align=\'center\'>" + sodao.Diq + "</td> </tr>";


                if (sodao.Diq <= 3)
                {
                    fo_aging_lessoreq_3 = fo_aging_lessoreq_3 + 1;
                    rem_lt_3_days = rem_lt_3_days + sodao.Remarks + " ";
                }
                else if (sodao.Diq > 3 && sodao.Diq < 11)
                {
                    fo_aging_4to10 = fo_aging_4to10 + 1;
                    rem_4to10_days = rem_4to10_days + sodao.Remarks + " ";
                }
                else if (sodao.Diq > 10)
                {
                    fo_aging_gt_10 = fo_aging_gt_10 + 1;
                    rem_gt10_days = rem_gt10_days + sodao.Remarks + " ";
                }

            }


            aging_html = aging_html + "</table><BR/>";



            //===================== Purchase Parts AGING ===============

            int pp_aging_lessoreq_3 = 0;
            int pp_aging_4to10 = 0;
            int pp_aging_gt_10 = 0;


            ArrayList ppList = doGetSOTKFOList(5);
            aging_html = aging_html + "<table width=" + table_width + " align=\'center\' border=\'1\'>  <tr bgcolor=\"#009999\"> <td align=\'center\' colspan=\'4\'> Purchase Parts Aging </td> </tr>" +
                 "<tr bgcolor=\"#FFFE33\"> <td    width=\'10%\' align=\'center\'> Material# </td> <td    width=\'10%\' align=\'center\'>PO#  </td> <td   width=\'20%\'  align=\'center\'> Remarks </td>    <td width=\'5%\' align=\'center\'  > DIQ </td>   </tr> ";

            for (int x = 0; x < ppList.Count; x++)
            {
                ProcAgingDao sodao = (ProcAgingDao)ppList[x];

                aging_html = aging_html + "<tr> <td align=\'center\'>" + sodao.MaterialNum + "</td>";
                aging_html = aging_html + "<td align=\'center\'>" + sodao.ProdOrdNum + "</td>";
                aging_html = aging_html + "<td align=\'left\'>" + sodao.Remarks + "</td>";
                aging_html = aging_html + "<td align=\'center\'>" + sodao.Diq + "</td> </tr>";


                if (sodao.Diq <= 3)
                {
                    pp_aging_lessoreq_3 = pp_aging_lessoreq_3 + 1;
                    rem_lt_3_days = rem_lt_3_days + sodao.Remarks + " ";
                }
                else if (sodao.Diq > 3 && sodao.Diq < 11)
                {
                    pp_aging_4to10 = pp_aging_4to10 + 1;
                    rem_4to10_days = rem_4to10_days + sodao.Remarks + " ";
                }
                else if (sodao.Diq > 10)
                {
                    pp_aging_gt_10 = pp_aging_gt_10 + 1;
                    rem_gt10_days = rem_gt10_days + sodao.Remarks + " ";
                }

            }
            aging_html = aging_html + "</table> <BR/>";


            //============ ====//#009999

            int all_so_count = so_aging_lessoreq_3 + so_aging_4to10 + so_aging_gt_10;
            int all_tk_count = tk_aging_lessoreq_3 + tk_aging_4to10 + tk_aging_gt_10;
            int all_fo_count = fo_aging_lessoreq_3 + fo_aging_4to10 + fo_aging_gt_10;

            //Total number of new parts bgcolor=\"#009999\"
            String count_html = "<h5> TPC Project Progress Update </h5>  ";
            count_html = count_html + "<table width=" + table_width + " align=\'center\' border=\'1\'> <tr bgcolor=\"#009999\"> <td align=\'center\' colspan=\'7\'> TPC Project Progress Update </td> </tr> " +
                "<tr bgcolor=\"#FFFE33\"> <td rowspan=\'2\'  width=\'10%\' align=\'center\'> Date </td> <td colspan=\'4\'  align=\'center\'> Total number of new parts </td>    <td style=\"background-color:#d6d6f5\"  width=\'10%\' align=\'center\' rowspan=\'2\'> All Parts - WIP </td> <td width=\'10%\' rowspan=\'2\'>  PInfo updated </td>  </tr> ";
            count_html = count_html + "<tr bgcolor=\"#FFFE33\"> <td width=\'10%\'  align=\'center\' > FO  </td> <td width=\'10%\' align=\'center\'> Turnkey  </td> <td width=\'10%\' align=\'center\'> SO   </td>  <td width=\'10%\' align=\'center\'> Purchase Parts   </td>    " +
                " " +
                "  </tr> ";



            count_html = count_html + "<tr>";
            ProcurementDIQDao dao = (ProcurementDIQDao)al[0];

            count_html = count_html + "<td align=\'center\'>" + dao.ProcDate + "</td>";
            count_html = count_html + "<td align=\'center\'>" + all_fo_count + "</td>";
            count_html = count_html + "<td align=\'center\'>" + all_tk_count + "</td>";
            count_html = count_html + "<td align=\'center\'>" + all_so_count + "</td>";
            count_html = count_html + "<td align=\'center\'>" + dao.PinfoInProgressCount + "</td>";
            count_html = count_html + "<td align=\'center\' style=\"background-color:#d6d6f5\" >" + (all_fo_count + all_tk_count + all_so_count + dao.PinfoInProgressCount) + "</td>";
            count_html = count_html + "<td align=\'center\'>" + dao.PinfoLineCount + "</td>";

            count_html = count_html + "</tr>";


            count_html = count_html + "</table>";
            count_html = count_html + "<BR/> <hr/>";

            //============ final assembly ============



            string html = count_html + "<table width=" + table_width + " align=\'center\' border=\'1\'>  <tr bgcolor=\"#009999\"> <td align=\'center\' colspan=\'7\'> Aging </td>  </tr>  <tr bgcolor=\"#FFFE33\"> <td align=\'center\' colspan=\'1\'> Based on KPI </td>  <td align=\'center\' colspan=\'5\'> By Category </td>  <td align=\'center\'rowspan=\'2\'> Remarks  </td> </tr>" +
                "<tr bgcolor=\"#FFFE33\"> <td    width=\'15%\' align=\'center\'>Aging new TPC Part</td> <td    width=\'8%\' align=\'center\'>FO   </td> <td   width=\'8%\'  align=\'center\'> TK </td>    <td width=\'8%\' align=\'center\'  > SO </td>   <td width=\'8%\' align=\'center\'  > PP </td>   <td style=\"background-color:#d6d6f5\" width=\'8%\' align=\'center\'  >Total </td> </tr> ";

            html = html + "<tr> <td align=\'center\'> <=3 Days </td> <td align=\'center\'> " + fo_aging_lessoreq_3 + "</td>  <td align=\'center\'> " + tk_aging_lessoreq_3 + "</td>   <td align=\'center\'> " + so_aging_lessoreq_3 + "</td>  <td align=\'center\'> " + pp_aging_lessoreq_3 + "</td>  <td style=\"background-color:#d6d6f5\" align=\'center\'> " + (so_aging_lessoreq_3 + tk_aging_lessoreq_3 + fo_aging_lessoreq_3+ pp_aging_lessoreq_3) + "</td>  <td align=\'left\'> "+ rem_lt_3_days + " </td></tr>";
            html = html + "<tr> <td align=\'center\'> 4-10 Days </td> <td align=\'center\'> " + fo_aging_4to10 + "</td>  <td align=\'center\'> " + tk_aging_4to10 + "</td>   <td align=\'center\'> " + so_aging_4to10 + "</td>  <td align=\'center\'> " + pp_aging_4to10 + "</td>  <td style=\"background-color:#d6d6f5\" align=\'center\'> " + (so_aging_4to10+ tk_aging_4to10 + fo_aging_4to10 + pp_aging_4to10) + "</td> <td align=\'left\'> "+ rem_4to10_days + " </td></tr>";
            html = html + "<tr> <td align=\'center\'> >10 Days </td> <td align=\'center\'> " + fo_aging_gt_10 + "</td>  <td align=\'center\'> " + tk_aging_gt_10 + "</td>   <td align=\'center\'> " + so_aging_gt_10 + "</td>  <td align=\'center\'> " + pp_aging_gt_10 + "</td>  <td style=\"background-color:#d6d6f5\" align=\'center\'> " + (so_aging_gt_10 + tk_aging_gt_10 + fo_aging_gt_10 + pp_aging_gt_10) + "</td>   <td align=\'left\'> "+ rem_gt10_days + " </td></tr>";

            html = html + "</table><BR/> <hr/>";



            html = html + aging_html +"<hr/>";

            /* ArrayList alz = getAllActiveEmails();
             if (alz.Count == 0)
             {
                 return;
             }
             else
             {
                 string cc_emails = string.Empty;
                 string to_email = string.Empty;
                 for (int x = 0; x < alz.Count; x++)
                 {
                     ThreadProtectorAlerEmailDao dao = (ThreadProtectorAlerEmailDao)alz[x];
                     if (x == 0)
                     {
                         to_email = dao.Email;
                     }
                     else
                     {
                         cc_emails = cc_emails + dao.Email + ";";
                     }

                 }


                 MYGlobal.sendHALSMTPEmail("All", "[Thread Protector Stock Update]", to_email, cc_emails, null, html);
             }*/


            string cc_emails = string.Empty;
            string to_email = string.Empty;
            string bcc_email = string.Empty;

            cc_emails  = MYGlobal.GetSettingValue("Procurement_cc_email");
            to_email = MYGlobal.GetSettingValue("Procurement_to_email");
            bcc_email = MYGlobal.GetSettingValue("Procurement_bcc_email");

            /*cc_emails = cc_emails  + " AndyTan@halliburton.com, " + "Nevice.Phang@halliburton.com," + "LeeLeeAnlycia.Ding@halliburton.com,"  
                + "SuatChingJessica.Loh@halliburton.com,"  + "ChoonLianCynthia.Goh@halliburton.com," + "KhinHnin.Aye@halliburton.com, HaiSoon.Yeo@halliburton.com,"
                + "Johnson.Lai@halliburton.com," + "SiewLing.Ooi@halliburton.com,Peihuang.Yuan@halliburton.com,Vidhya.Krishnamani@halliburton.com" ;
            to_email = to_email + " EngChong.Ng@halliburton.com, WeiMin.Wong@halliburton.com"  ;*/

            // to_email = "kamesh.shankaran@halliburton.com";

            MYGlobal.sendHALSMTPEmail("TPCAlert@Halliburton.com", "All", "[TPC] Progress update on P-info creation for TPC Project", to_email, cc_emails, bcc_email, html);

        }


        private static ArrayList doGetSOTKFOList(int action)
        {
            ArrayList al = new ArrayList();
            using (SqlConnection sqlCon = new SqlConnection(MYGlobal.getCString()))
            {
                sqlCon.Open();
                log.Info(" doGetSOTKFOList()  sql = SP_PROCUREMENT_DIQ_REPORT , Action=" + action);
                using (SqlCommand cmd = new SqlCommand("SP_PROCUREMENT_DIQ_REPORT", sqlCon))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ACTION", action);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                string resource = string.Empty;
                                string mtrlNum = string.Empty;
                                string prodOrdNum = string.Empty;
                                string remarks_procurement = string.Empty;

                                int diq = 0;
                                ProcAgingDao dao = new ProcAgingDao();

                                if (action == 5)
                                {
                                   
                                }
                                else
                                {
                                   /* if ((reader["resource_wc"]) != DBNull.Value)
                                    {
                                        resource = ((string)reader["resource_wc"]);
                                        dao.Resource = resource;
                                    }*/

                                }

                                if ((reader["material_num"]) != DBNull.Value)
                                {
                                    mtrlNum = ((string)reader["material_num"]);
                                    dao.MaterialNum = mtrlNum;
                                }

                                if ((reader["prod_ZEWO"]) != DBNull.Value)
                                {
                                    prodOrdNum = ((string)reader["prod_ZEWO"]);
                                    dao.ProdOrdNum = prodOrdNum;
                                }


                                if ((reader["remarks_procurement"]) != DBNull.Value)
                                {
                                    remarks_procurement = ((string)reader["remarks_procurement"]);
                                    dao.Remarks = remarks_procurement;
                                }

                                if ((reader["diq"]) != DBNull.Value)
                                {
                                    diq = ((int)reader["diq"]);
                                    dao.Diq = diq;
                                }

                                al.Add(dao);

                                log.Info("PP resource=" + resource + ", remarks_procurement=" + remarks_procurement + ", diq = " + diq);
                            }catch(Exception ee)
                            {
                                log.Error("Error = " + ee.Message);
                            }
                        }
                    }
                   
                }
            }

            return al;
        }


        private static ArrayList doGetAllProc()
        {
            ArrayList al = new ArrayList();
           
            try
            {

                using (SqlConnection sqlCon = new SqlConnection(MYGlobal.getCString()))
                {
                    sqlCon.Open();
                    log.Info(" doGetAllProc()  sql = SP_PROCUREMENT_DIQ_REPORT");
                    using (SqlCommand cmd = new SqlCommand("SP_PROCUREMENT_DIQ_REPORT", sqlCon))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ACTION", 1);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {

                           
                            while (reader.Read())
                            {
                                ProcurementDIQDao dao = new ProcurementDIQDao();
                                
                                if ((reader["SO_COUNT"]) != DBNull.Value)
                                {
                                    dao.SoCount = ((int)reader["SO_COUNT"]);
                                }
                                if ((reader["SO_LINE_COUNT"]) != DBNull.Value)
                                {
                                    dao.SoLineCount = ((int)reader["SO_LINE_COUNT"]);
                                }

                                if ((reader["FO_COUNT"]) != DBNull.Value)
                                {
                                    dao.FoCount = ((int)reader["FO_COUNT"]);
                                }
                                if ((reader["FO_LINE_COUNT"]) != DBNull.Value)
                                {
                                    dao.FoLineCount = ((int)reader["FO_LINE_COUNT"]);
                                }

                                if ((reader["TK_COUNT"]) != DBNull.Value)
                                {
                                    dao.TKCount = ((int)reader["TK_COUNT"]);
                                }
                                if ((reader["TK_LINE_COUNT"]) != DBNull.Value)
                                {
                                    dao.TKLineCount = ((int)reader["TK_LINE_COUNT"]);
                                }

                                if ((reader["PINFO_INPROGRESS_COUNT"]) != DBNull.Value)
                                {
                                    dao.PinfoInProgressCount = ((int)reader["PINFO_INPROGRESS_COUNT"]);
                                }

                                if ((reader["PINFO_DONE_TODAY_COUNT"]) != DBNull.Value)
                                {
                                    dao.PinfoLineCount = ((int)reader["PINFO_DONE_TODAY_COUNT"]);
                                }

                                dao.ProcDate = DateTime.Now.ToString("yyyy-MM-dd");
                                log.Info("FOCount="+dao.FoCount +", FOLineCount="+dao.FoLineCount +", SOCount="+dao.SoCount 
                                    +", SOLineCount="+dao.SoLineCount +", TKCount="+dao.TKCount +", TKLineCount="+dao.TKLineCount);

                                al.Add( dao);
                            }
                        }
                    }
                }

                return al;

            }
            catch(Exception ee)
            {
                log.Error("Exception = " + ee.Message);
            }

            return al;
        }
    }
}
