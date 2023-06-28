using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPC2UpdaterApp.DB;

/**
 *  Get all QC Done and WH Received for today
 *  7Am for yesterday records
 *  4pm for Today records
 * 
 */
namespace TPC2UpdaterApp.warehouse
{
  public  class WHUpdator
    {

        //  select top 100 qc_done_on, qc_done_by, GR_done_by, GR_done_on,  tracking_number, sap_network, material_num, total_req_qty, material_desc, form_type, tech_project_mgr, tech_resp_engineer, * 
        //from view_t2_project_material where qc_done = 1 and qc_done_on >='2022-10-05';

        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void doWarehouseQC(DateTime date, string ampm)
        {
            ArrayList alQC = doAllMaterial4QC(date);

            ArrayList alWH = doAllMaterial4WH(date);
            /* for(int x = 0; x < al.Count; x++)
             {
                 T2ProjectMaterialDao dao = (T2ProjectMaterialDao)al[x];
                 log.Info(" tracking num "+dao.TrackingNumber +", Material num="+ dao.MaterialNbr +", ");
                 //send email
             }*/

            if(alQC.Count > 0 || alWH.Count > 0)
            {
                doHtml(alQC, alWH, date, ampm);
            }
            else
            {
                log.Info("Both QC and WH are 0");
                doEmptyEmail( date,  ampm);
            }

            
        }

        public static void doEmptyEmail(DateTime date, string ampm)
        {
            string cc_emails = "kamesh.shankaran@halliburton.com";
            string to_email = "TanKok.Wah@halliburton.com";
            string bcc_email = cc_emails;
            //===================

            string html = "<h1> Both QC and WH are empty </h1>";

            MYGlobal.sendHALSMTPEmail("TPCAlert@Halliburton.com", "All", "[TPC] QC & WH done on [" + date.ToString("yyyy-MM-dd") + "] " + ampm +" [EMPTY]", to_email, cc_emails, bcc_email, html);

            log.Info("Email sent \n\n");
        }

       //whenever email sent update this in_planner_excel column

        static string table_width = "95%";
        private static void doHtml(ArrayList qcList, ArrayList whList, DateTime date, string ampm)
        {
            string aging_html = "";

            try
            {
                //Quick count
                aging_html = aging_html + "<span style='font-size:22; color:blue'> QC completed count : " + qcList.Count + " lines.</span> <br/> <span style='font-size:22; color:blue'> WH received count : " + whList.Count + " lines. </span> <br/>";

                if (qcList.Count > 0)
                {
                    //Quick note
                    aging_html = aging_html + "<h5> <u> Inspection completed for the below items, Please collect the parts at 9.30AM or 2.30PM  only </u> </h5>  ";

                    //QC body
                    aging_html = aging_html + "<table width=" + table_width + " align=\'center\' border=\'1\'>  <tr bgcolor=\"#009999\"> <td align=\'center\' colspan=\'15\' style='font-size:25;'> QC done list </td> </tr>" +
                        "<tr bgcolor=\"#FFFE33\"> <td    width=\'5%\' align=\'center\'> Tracking# </td> <td    width=\'5%\' align=\'center\'>SAP Network#  </td> " +
                        "<td   width=\'3%\'  align=\'center\'> Act Code </td>    <td width=\'5%\' align=\'center\'  > Material Num# </td> <td width=\'3%\' align=\'center\'> Actual Qty </td>" +
                        "<td   width=\'5%\'  align=\'center\'> QC Done Today </td> <td   width=\'5%\'  align=\'center\'> QC Done Total </td> <td   width=\'5%\'  align=\'center\' style='color:red'> Balance Qty </td>" +
                        "<td width=\'10%\' align=\'center\'  > QC Remarks </td> <td width=\'10%\' align=\'center\'  > Material Desc </td>  <td width=\'3%\' align=\'center\'  > Form Type </td>  <td width=\'5%\' align=\'center\'  > Prod Zewo </td> " +
                        "<td width=\'5%\' align=\'center\'  > Proj Mgr </td>  <td width=\'5%\' align=\'center\'  > Engineer </td> <td width=\'5%\' align=\'center\'  > QC Done on </td> " +
                        " </tr> ";
                    int count = qcList.Count;
                    for (int x = 0; x < qcList.Count; x++)
                    {
                        T2ProjectMaterialDao sodao = (T2ProjectMaterialDao)qcList[x];

                        aging_html = aging_html + "<tr> <td align=\'center\'>" + sodao.TrackingNumber + "</td>";
                        aging_html = aging_html + "<td align=\'center\'>" + sodao.SAPNetwork + "</td>";
                        aging_html = aging_html + "<td align=\'left\'>" + sodao.ActCode + "</td>";
                        aging_html = aging_html + "<td align=\'center\'>" + sodao.MaterialNbr + "</td>  ";
                        aging_html = aging_html + "<td align=\'center\'>" + sodao.TotalReqQty + "</td>  ";
                        aging_html = aging_html + "<td align=\'center\'>" + sodao.QCQtyDoneToday + "</td>  ";
                        aging_html = aging_html + "<td align=\'center\'>" + sodao.QCQtyDoneTotal + "</td>  ";
                        aging_html = aging_html + "<td align=\'center\' style='color:red'>" + sodao.QCQtyBalance + "</td>  ";
                        aging_html = aging_html + "<td align=\'center\'>" + sodao.QCRemarks + "</td> ";
                        aging_html = aging_html + "<td align=\'center\'>" + sodao.MaterialDesc + "</td> ";
                        aging_html = aging_html + "<td align=\'center\'>" + sodao.FormType + "</td>  ";
                        aging_html = aging_html + "<td align=\'center\'>" + sodao.ProdZEWO + "</td> ";
                        aging_html = aging_html + "<td align=\'center\'>" + sodao.TechProjMgr + "</td> ";
                        aging_html = aging_html + "<td align=\'center\'>" + sodao.TechRespEngr + "</td> ";
                        try
                        {
                            aging_html = aging_html + "<td align=\'center\'>" + sodao.QCDoneOn.ToString("yyyy-MM-dd") + "</td> ";
                        }
                        catch (Exception ee)
                        {
                            log.Info("ee " + ee.Message);
                        }
                        //aging_html = aging_html + "<td align=\'center\'>" + sodao.QCDoneBy + "</td> ";
                        aging_html = aging_html + "</ tr >";

                    }//for , last row

                    aging_html = aging_html + "<tr> <td  colspan=\'15\' align=\'center\' bgcolor=\"#FFFE33\"> Total Lines :  " + count + "</td> </tr>";

                    aging_html = aging_html + "</table><BR/><BR/>";

                    //aging_html = aging_html + " < h1> Total Lines "+count +"</h1>";

                }


                if (whList.Count > 0)
                {
                    aging_html = aging_html + "<h5> <u> The following items are received by Tech Warehouse. </u> </h5>  ";
                    aging_html = aging_html + "<table width=" + table_width + " align=\'center\' border=\'1\'>  <tr bgcolor=\"#009999\"> <td align=\'center\' colspan=\'11\' style='font-size:25;'>  WH received list </td> </tr>" +
                        "<tr bgcolor=\"#FFFE33\"> <td    width=\'5%\' align=\'center\'> Tracking# </td> <td    width=\'5%\' align=\'center\'>SAP Network#  </td> " +
                        "<td   width=\'3%\'  align=\'center\'> Act Code </td>    <td width=\'5%\' align=\'center\'  > Material Num# </td> <td width=\'3%\' align=\'center\'  > Qty </td>" +
                        "<td width=\'15%\' align=\'center\'  > Material Desc </td>  <td width=\'3%\' align=\'center\'  > Form Type </td>  <td width=\'5%\' align=\'center\'  > Prod Zewo </td> " +
                        "<td width=\'5%\' align=\'center\'  > Proj Mgr </td>  <td width=\'5%\' align=\'center\'  > Engineer </td> <td width=\'5%\' align=\'center\'  > WH Recd on </td> " +
                        " </tr> ";
                    int count = whList.Count;
                    for (int x = 0; x < whList.Count; x++)
                    {
                        T2ProjectMaterialDao sodao = (T2ProjectMaterialDao)whList[x];

                        aging_html = aging_html + "<tr> <td align=\'center\'>" + sodao.TrackingNumber + "</td>";
                        aging_html = aging_html + "<td align=\'center\'>" + sodao.SAPNetwork + "</td>";
                        aging_html = aging_html + "<td align=\'left\'>" + sodao.ActCode + "</td>";
                        aging_html = aging_html + "<td align=\'center\'>" + sodao.MaterialNbr + "</td>  ";
                        aging_html = aging_html + "<td align=\'center\'>" + sodao.TotalReqQty + "</td>  ";
                        aging_html = aging_html + "<td align=\'center\'>" + sodao.MaterialDesc + "</td> ";
                        aging_html = aging_html + "<td align=\'center\'>" + sodao.FormType + "</td>  ";
                        aging_html = aging_html + "<td align=\'center\'>" + sodao.ProdZEWO + "</td> ";
                        aging_html = aging_html + "<td align=\'center\'>" + sodao.TechProjMgr + "</td> ";
                        aging_html = aging_html + "<td align=\'center\'>" + sodao.TechRespEngr + "</td> ";
                        try
                        {
                            aging_html = aging_html + "<td align=\'center\'>" + sodao.WHDoneOn.ToString("yyyy-MM-dd") + "</td> ";
                        }
                        catch (Exception ee)
                        {
                            log.Info("ee " + ee.Message);
                        }
                        //aging_html = aging_html + "<td align=\'center\'>" + sodao.QCDoneBy + "</td> ";
                        aging_html = aging_html + "</ tr >";

                    }//for , last row

                    aging_html = aging_html + "<tr> <td  colspan=\'11\' align=\'center\' bgcolor=\"#FFFE33\"> Total Lines :  " + count + "</td> </tr>";

                    aging_html = aging_html + "</table><BR/>";

                    //aging_html = aging_html + " < h1> Total Lines "+count +"</h1>";
                }



                string html = aging_html;


                string cc_emails = string.Empty;
                string to_email = string.Empty;
                string bcc_email = string.Empty;

                //use cc email from config
               cc_emails = "" +
                      "Visvanathan.Muniandy2@halliburton.com;TanKok.Wah@halliburton.com;SoonSeng.Wong@halliburton.com;MdZainalAbidin.Yusof@halliburton.com;WeiMin.Wong@halliburton.com;Prabhu.Velusamy@halliburton.com;KoonHaw.Lim@halliburton.com;" +
                      "Cheng.Zhang@halliburton.com;Zabid.MatYusop@halliburton.com;DavidYawEn.Koh@halliburton.com;thachanamoorthy.palaniandy@halliburton.com";
                log.Info("str_mgr_email_list " + str_mgr_email_list);
                to_email = str_mgr_email_list + DBUtils.getAllUserEmails(MYGlobal.ROLE_TPC_COORDINATOR_ID);
                log.Info("To emails " + to_email);

                bcc_email = MYGlobal.GetSettingValue("Procurement_bcc_email");

                //================ Test =========

                 /* cc_emails = "kamesh.shankaran@halliburton.com";
                     to_email = cc_emails;
                     bcc_email = cc_emails;  */
                //===================

                MYGlobal.sendHALSMTPEmail("TPCAlert@Halliburton.com", "All", "[TPC] QC & WH done on [" + date.ToString("yyyy-MM-dd") + "] " + ampm, to_email, cc_emails, bcc_email, html);

                log.Info("Email sent \n\n");

                doUpdateFlag(qcList);
            }catch(Exception ee)
            {
                log.Info("ee "+ee.Message);
            }
        }


        private static void doUpdateFlag(ArrayList al)
        {
            using (SqlConnection sqlCon = new SqlConnection(MYGlobal.getCString()))
            {
                sqlCon.Open();
               
                string sqlupdate = "update t2_material set in_planner_excel=1 where id=@mid";

                for (int x=0;x<al.Count;x++) {
                    T2ProjectMaterialDao sodao = (T2ProjectMaterialDao)al[x];

                    log.Info(" doUpdateFlag()  sqlupdate  " + sqlupdate +" for mid="+ sodao.MatlId);
                    //update
                    using (SqlCommand cmd2 = new SqlCommand(sqlupdate, sqlCon))
                    {
                        cmd2.CommandType = CommandType.Text;
                        cmd2.Parameters.AddWithValue("@mid", sodao.MatlId);

                        int rows = cmd2.ExecuteNonQuery();
                        log.Info("mid=" + sodao.MatlId + ",  rows = " + rows);

                    }//
                }
            }
        }


        static HashSet<string> mgr_email_list = new HashSet<string>();
        static string str_mgr_email_list = "";
        //static HashSet<string> engr_email_list = new HashSet<string>();



        private static ArrayList doAllMaterial4QC(DateTime date)
        {
            //string sql = "  select top 10 qc_done_on, qc_done_by,activity_code, prod_zewo, GR_done_by, GR_done_on, tracking_number, sap_network, material_num, total_req_qty, material_desc, form_type, tech_project_mgr, " +
            //  "tech_resp_engineer  from view_t2_project_material where qc_done = 1 and qc_done_on >= '2022-10-05';  ";

            string sql = "select  m.id as mid,  p.proj_mgr_email, p.engr_email,  m.material_num, m.qc_done_on, m.qc_done_by, p.activity_code, m.prod_zewo, m.GR_done_by, m.GR_done_on, " +
                " p.tracking_number,   p.sap_network, m.material_num, m.total_req_qty, m.material_desc, p.form_type, p.proj_mgr_name, p.engr_name,  " +
                "m.qc_qty_done, m.qc_qty_total_done,  m.qc_qty_balance, m.qc_remarks " +
                "from view_t2_project_user p, t2_material m where p.id = m.project_id and m.qc_done in (1,2) and m.qc_done_on ='"+ date.ToString("yyyy-MM-dd")+ "' and in_planner_excel is null";
                                 
            ArrayList al = new ArrayList();
            using (SqlConnection sqlCon = new SqlConnection(MYGlobal.getCString()))
            {
                sqlCon.Open();
                log.Info(" doAllMaterial4QCWH()  sql  " + sql);
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

                                //int diq = 0;

                                T2ProjectMaterialDao dao = new T2ProjectMaterialDao();

                                dao.MatlId = (int)reader["mid"];

                                if ((reader["tracking_number"]) != DBNull.Value)
                                {
                                    dao.TrackingNumber = ((string)reader["tracking_number"]);                                     
                                }

                                try
                                {
                                    if ((reader["qc_done_on"]) != DBNull.Value)
                                    {
                                        dao.QCDoneOn = ((DateTime)reader["qc_done_on"]);
                                    }

                                    if ((reader["qc_done_by"]) != DBNull.Value)
                                    {
                                        dao.QCDoneBy = ((string)reader["qc_done_by"]);
                                    }
                                }catch(Exception ee)
                                {
                                    log.Error("ee " + ee.Message);
                                }

                                try { 
                                    if ((reader["GR_done_on"]) != DBNull.Value)
                                    {
                                        dao.WHDoneOn = ((DateTime)reader["GR_done_on"]);
                                    }

                                    if ((reader["GR_done_by"]) != DBNull.Value)
                                    {
                                        dao.WHDoneBy = ((string)reader["GR_done_by"]);
                                    }
                                }
                                catch (Exception ee)
                                {
                                    log.Error("ee " + ee.Message);
                                }


                                if ((reader["engr_email"]) != DBNull.Value)
                                {
                                    string email = (string)reader["engr_email"];
                                    if (!mgr_email_list.Contains((email))){
                                        mgr_email_list.Add(email);
                                       // mgr_email_list.Add((string)reader["engr_email"]);
                                        str_mgr_email_list = str_mgr_email_list + email + ";";
                                    }
                                }

                                if ((reader["proj_mgr_email"]) != DBNull.Value)
                                {
                                    string email = (string)reader["proj_mgr_email"];
                                    if (!mgr_email_list.Contains((email)))
                                    {
                                        mgr_email_list.Add(email);
                                        str_mgr_email_list = str_mgr_email_list + email + ";";
                                    }
                                }

                                if ((reader["sap_network"]) != DBNull.Value)
                                {
                                    dao.SAPNetwork = ((string)reader["sap_network"]);
                                    
                                }

                                if ((reader["activity_code"]) != DBNull.Value)
                                {
                                    dao.ActCode = ((string)reader["activity_code"]);

                                }

                                if ((reader["material_num"]) != DBNull.Value)
                                {
                                    dao.MaterialNbr = ((string)reader["material_num"]);
                                    
                                }

                                //m.qc_qty_done, m.qc_qty_total_done,  m.qc_qty_balance 
                                if ((reader["total_req_qty"]) != DBNull.Value)
                                {
                                    dao.TotalReqQty = ((int)reader["total_req_qty"]);                                    
                                }

                                if ((reader["qc_qty_done"]) != DBNull.Value)
                                {
                                    dao.QCQtyDoneToday = ((int)reader["qc_qty_done"]);
                                }

                                if ((reader["qc_qty_total_done"]) != DBNull.Value)
                                {
                                    dao.QCQtyDoneTotal = ((int)reader["qc_qty_total_done"]);
                                }

                                if ((reader["qc_qty_balance"]) != DBNull.Value)
                                {
                                    dao.QCQtyBalance = ((int)reader["qc_qty_balance"]);
                                }

                                if ((reader["qc_remarks"]) != DBNull.Value)
                                {
                                    dao.QCRemarks = ((string)reader["qc_remarks"]);

                                }

                                if ((reader["material_desc"]) != DBNull.Value)
                                {
                                    dao.MaterialDesc = ((string)reader["material_desc"]); 

                                }

                                if ((reader["prod_zewo"]) != DBNull.Value)
                                {
                                    dao.ProdZEWO = ((string)reader["prod_zewo"]);

                                }

                                if ((reader["form_type"]) != DBNull.Value)
                                {
                                    dao.FormType = ((string)reader["form_type"]);

                                }

                                if ((reader["proj_mgr_name"]) != DBNull.Value)
                                {
                                    dao.TechProjMgr = ((string)reader["proj_mgr_name"]);

                                }

                                if ((reader["engr_name"]) != DBNull.Value)
                                {
                                    dao.TechRespEngr = ((string)reader["engr_name"]);

                                }


                                al.Add(dao);

                               // log.Info("PP ");
                            }
                            catch (Exception ee)
                            {
                                log.Error("Error = " + ee.Message);
                            }
                        }
                    }

                }
            }

            return al;
        }

        //update t2_material set production_order_num=0 where id=16266;

        // select top 20 max(id),project_id from t2_material where material_status in ('Completed' , 'In-Progress') and desc_1 is not null
        //group by project_id order by project_id desc;
        //Temporariy update cost No Use
        public static void doTempUpdate()
        {
            string sql = " select top 2000 max(id) as mid, project_id  as pid from t2_material where material_status in ('Completed' , 'In-Progress') " +
                " and desc_1 is not null group by project_id order by project_id desc";

            string sqlupdate = "update t2_material set production_order_num=0 where id=@mid;";

            ArrayList al = new ArrayList();
            using (SqlConnection sqlCon = new SqlConnection(MYGlobal.getCString()))
            {
                sqlCon.Open();
                log.Info(" doAllMaterial4QCWH()  sql  " + sql);
                using (SqlCommand cmd = new SqlCommand(sql, sqlCon))
                {
                    cmd.CommandType = CommandType.Text;
                    //cmd.Parameters.AddWithValue("@ACTION", action);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if ((reader["mid"]) != DBNull.Value)
                            {
                                int mid = ((int)reader["mid"]);
                                al.Add(mid);
                            }
                        }
                    }//
                }//
            }//connection

            log.Info("total lines " + al.Count);

            using (SqlConnection sqlCon = new SqlConnection(MYGlobal.getCString()))
            {
                sqlCon.Open();
                //update
                for (int x = 0; x < al.Count; x++)
                {
                    int xp = (int)al[x];

                    //update
                    using (SqlCommand cmd2 = new SqlCommand(sqlupdate, sqlCon))
                    {
                        cmd2.CommandType = CommandType.Text;
                        cmd2.Parameters.AddWithValue("@mid", xp);
                         
                         int rows = cmd2.ExecuteNonQuery();
                         log.Info("doTempUpdate() mid=" + xp + ",  rows = " + rows);
                        
                    }//

                }//for
            }//conn

            
        }

        private static ArrayList doAllMaterial4WH(DateTime date)
        {
            //string sql = "  select top 10 qc_done_on, qc_done_by,activity_code, prod_zewo, GR_done_by, GR_done_on, tracking_number, sap_network, material_num, total_req_qty, material_desc, form_type, tech_project_mgr, " +
            //  "tech_resp_engineer  from view_t2_project_material where qc_done = 1 and qc_done_on >= '2022-10-05';  ";

            string sql = "select top 100 p.proj_mgr_email, p.engr_email,  m.material_num,  p.activity_code, m.prod_zewo, m.GR_done_by, m.GR_done_on, " +
                " p.tracking_number,   p.sap_network, m.material_num, m.total_req_qty, m.material_desc, p.form_type, p.proj_mgr_name, p.engr_name  " +
                " from view_t2_project_user p, t2_material m where p.id = m.project_id and m.GR_done_by is not null and m.gr_done_on ='" + date.ToString("yyyy-MM-dd") + "' ";



            ArrayList al = new ArrayList();
            using (SqlConnection sqlCon = new SqlConnection(MYGlobal.getCString()))
            {
                sqlCon.Open();
                log.Info(" doAllMaterial4QCWH()  sql  " + sql);
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


                                //int diq = 0;

                                T2ProjectMaterialDao dao = new T2ProjectMaterialDao();
                                if ((reader["tracking_number"]) != DBNull.Value)
                                {
                                    dao.TrackingNumber = ((string)reader["tracking_number"]);
                                }                               

                                try
                                {
                                    if ((reader["GR_done_on"]) != DBNull.Value)
                                    {
                                        dao.WHDoneOn = ((DateTime)reader["GR_done_on"]);
                                    }

                                    if ((reader["GR_done_by"]) != DBNull.Value)
                                    {
                                        dao.WHDoneBy = ((string)reader["GR_done_by"]);
                                    }
                                }
                                catch (Exception ee)
                                {
                                    log.Error("ee " + ee.Message);
                                }


                                if ((reader["engr_email"]) != DBNull.Value)
                                {
                                    string email = (string)reader["engr_email"];
                                    if (!mgr_email_list.Contains((email)))
                                    {
                                        mgr_email_list.Add(email);
                                        // mgr_email_list.Add((string)reader["engr_email"]);
                                        str_mgr_email_list = str_mgr_email_list + email + ";";
                                    }
                                }

                                if ((reader["proj_mgr_email"]) != DBNull.Value)
                                {
                                    string email = (string)reader["proj_mgr_email"];
                                    if (!mgr_email_list.Contains((email)))
                                    {
                                        mgr_email_list.Add(email);
                                        str_mgr_email_list = str_mgr_email_list + email + ";";
                                    }
                                }

                                if ((reader["sap_network"]) != DBNull.Value)
                                {
                                    dao.SAPNetwork = ((string)reader["sap_network"]);

                                }

                                if ((reader["activity_code"]) != DBNull.Value)
                                {
                                    dao.ActCode = ((string)reader["activity_code"]);

                                }

                                if ((reader["material_num"]) != DBNull.Value)
                                {
                                    dao.MaterialNbr = ((string)reader["material_num"]);

                                }

                                if ((reader["total_req_qty"]) != DBNull.Value)
                                {
                                    dao.TotalReqQty = ((int)reader["total_req_qty"]);

                                }

                                if ((reader["material_desc"]) != DBNull.Value)
                                {
                                    dao.MaterialDesc = ((string)reader["material_desc"]);

                                }

                                if ((reader["prod_zewo"]) != DBNull.Value)
                                {
                                    dao.ProdZEWO = ((string)reader["prod_zewo"]);

                                }

                                if ((reader["form_type"]) != DBNull.Value)
                                {
                                    dao.FormType = ((string)reader["form_type"]);

                                }

                                if ((reader["proj_mgr_name"]) != DBNull.Value)
                                {
                                    dao.TechProjMgr = ((string)reader["proj_mgr_name"]);

                                }

                                if ((reader["engr_name"]) != DBNull.Value)
                                {
                                    dao.TechRespEngr = ((string)reader["engr_name"]);

                                }


                                al.Add(dao);

                                //log.Info("PP ");
                            }
                            catch (Exception ee)
                            {
                                log.Error("Error = " + ee.Message);
                            }
                        }
                    }

                }
            }

            return al;
        }

    }
}
