using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace TPC2UpdaterApp.DB
{
    class DBUtils
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static String TEMPLOG_DB = "";
        public static String GAGETRACK_DB = "";

        public static SqlConnection getSQLConnection()
        {
            SqlConnection cnn = null;
            try
            {
                //string connStr = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
                //Response.Write("DB Connection String = " + connStr);
                string connStr = MYGlobal.getCString();
                cnn = new SqlConnection(connStr);
                cnn.Open();
            }
            catch (Exception ex)
            {
                log.Error("Connection failed " + ex);
            }

            return cnn;
        }

        public static String getMaterialMasterDbCString()
        {
            return @"Data Source=HCTNADBS001;Initial Catalog=CPS_DATA;User ID=TPC_APP_USER;Password=R4b*A6t#S8q~";
        }


        public static SqlConnection getMaterialMasterSQLConnection()
        {
            SqlConnection cnn = null;
            try
            {
                string connStr = getMaterialMasterDbCString();
                cnn = new SqlConnection(connStr);
                cnn.Open();
            }
            catch (Exception ex)
            {
                log.Info("Connection failed " + ex);
                return null;
            }

            return cnn;
        }


        public static bool insertUsageHistoryHelper(String action, string remakrs)
        {
            try
            { 
                UsageHistoryDao dao = new UsageHistoryDao();
                dao.Hid = "System";
                dao.Name = "Updator_System";
                dao.Action = action;
                dao.ActionTime = DateTime.Now;
                dao.Remarks = remakrs;
                bool bb = insertUsageHistory(dao);
                if (bb)
                {
                    return true;
                }
            }
            catch (Exception ee)
            {
                log.Error(" " + ee.Message);
            }
            finally
            {
                //MYGlobal.DOClose(con, adapter);
            }
            return false;
        }

        public static bool insertUsageHistory(UsageHistoryDao dao)
        {
            SqlConnection con = null;
            SqlDataAdapter adapter = null;
            try
            {


                con = getSQLConnection();
                adapter = new SqlDataAdapter();
                String sql = "insert into  t_usage_history " 
                    + " (hid, name, action, action_time, remarks)"
                    + " values(@hid, @name, @action, @action_time, @remarks)";

                log.Info("insertUsageHistory SQL  " + sql);

                adapter.InsertCommand = new SqlCommand(sql, con);
                adapter.InsertCommand.Parameters.AddWithValue("@hid", dao.Hid);
                adapter.InsertCommand.Parameters.AddWithValue("@name", dao.Name);
                adapter.InsertCommand.Parameters.AddWithValue("@action", dao.Action);
                adapter.InsertCommand.Parameters.AddWithValue("@action_time", dao.ActionTime);
                adapter.InsertCommand.Parameters.AddWithValue("@remarks", dao.Remarks);

                adapter.InsertCommand.ExecuteNonQuery();

                //adapter.InsertCommand.Dispose();
                log.Info("insertUsageHistory Success");

                return true;
            }
            catch (Exception ex)
            {
                log.Error("insertUsageHistory Error : " + ex.ToString());
            }
            finally
            {
                MYGlobal.DOClose(con, adapter);
            }

            return false;
        }

        //=========================


        public static String getTemplogDbCString()
        {
            TEMPLOG_DB = MYGlobal.GetSettingValue("TEMPLOGDB");

            if (TEMPLOG_DB.Equals("APMFG"))
            {
                return @"Data Source=APMFGDBS001;Initial Catalog=SING3_TEMPLOG;User ID=CIMS_USER;Password=D2c?Z7w^E8e!";
            }
            return string.Empty;
        }


        public static String getGageTrackDbCString()
        {
            GAGETRACK_DB = MYGlobal.GetSettingValue("GAGETRACKG_SERVER");

            if (GAGETRACK_DB.Equals("SINAS2"))
            {
                return @"Data Source=SINASRS102;Initial Catalog=GAGETRAK65;User ID=GTLOGIN;Password=PASSword1";
            }
            return string.Empty;
        }



        public static String getAllUserEmails(int roleId)
        {
            String emails = "";
            SqlConnection cnn = null;
            SqlCommand cmd = null;
            SqlDataReader reader;
            String sql = "";
            String order_by = " order by id desc";
            try
            {


                sql = "select email from " + MYGlobal.TABLE_USER + " WITH (NOLOCK) where role_id=" + roleId;


                //add order by
                sql = sql + order_by;
                log.Info("getAllUserEmails sql = " + sql);

                cnn = getSQLConnection();
                cmd = new SqlCommand(sql, cnn);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    //  MessageBox.Show(reader.GetValue(0) + " - " + reader.GetValue(1) + " - " + reader.GetValue(2));
                    String email = (String)reader.GetValue(0);
                    emails = emails + email + ";";
                }
                reader.Close();
                cmd.Dispose();
                cnn.Close();
            }
            catch (Exception ex)
            {
                log.Error("Can not open connection ! " + ex);
            }
            finally
            {
                MYGlobal.DOClose(cnn, null);
            }

            return emails;
        }






        public static int doInsertUpdateT2Material4Planner(T2MaterialDao dao)
        {
            int affRows = 0;
            try
            {
                log.Info("doInsertUpdateT2Material4Planner()   for  ProjectId=" + dao.ProjectId + ", matlid=" + dao.Id 
                    + ", current date=" + DateTime.Now);

                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();

                    using (SqlCommand cmd = new SqlCommand("[SP_UPDATE_T2_MATERIAL_FOR_PLANNER]", cnn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        //params                       

                        if (dao.Id > 0)
                        {
                            cmd.Parameters.AddWithValue("@ID", dao.Id);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@ID", 0);
                        }

                        cmd.Parameters.AddWithValue("@PROJECT_ID", dao.ProjectId);
                        cmd.Parameters.AddWithValue("@UNCONFIRMED_OPER", dao.UNConfirmWC);

                        if (dao.WCDeliveryDate.Year >= 2000)
                        {
                            cmd.Parameters.AddWithValue("@WC_DEV_DATE", dao.WCDeliveryDate);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@WC_DEV_DATE", DBNull.Value);
                        }

                        if (dao.WCPromisedDate.Year >= 2000)
                        {
                            cmd.Parameters.AddWithValue("@WC_PROMISED_DATE", dao.WCPromisedDate);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@WC_PROMISED_DATE", DBNull.Value);
                        }


                        // cmd.Parameters.AddWithValue("@WC_DEV_DATE", dao.WCDeliveryDate);
                        cmd.Parameters.AddWithValue("@WC_OWNER", dao.WCOwner);
                        cmd.Parameters.AddWithValue("@WC_REMARKS", dao.WCRemarks);
                        cmd.Parameters.AddWithValue("@WC_VENDOR", dao.WCVendor);

                        log.Info("values = @PROJECT_ID=" + dao.ProjectId + ", @UNCONFIRMED_OPER="+ dao.UNConfirmWC+ ", @WC_DEV_DATE="+ dao.WCDeliveryDate
                            + ", @WC_PROMISED_DATE="+ dao.WCPromisedDate + ",@WC_OWNER="+ dao.WCOwner + ",@WC_REMARKS="+ dao.WCRemarks + ",@WC_VENDOR="+ dao.WCVendor);


                        //exe
                        affRows = cmd.ExecuteNonQuery();
                        log.Info("Affrows = " + affRows);
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error("Error in doInsertUpdateT2Material4Planner() " + ex);
                return -2;
                throw ex;
            }


            return affRows;
        }


        public static ArrayList getAllt2ProjectAndMaterailsForPlanner(String plant, String ptnumber, String material_number)
        {
            ArrayList al = new ArrayList();


            try
            {
                //p.plant = @plant and
                String sql = SQL_PROJECT_MATERIAL + " where  p.tracking_number=@ptnum and  m.material_num=@matl_num";

                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.CommandType = CommandType.Text;
                        log.Info("getAllt2ProjectAndMaterailsForPlanner() query : Tracking Num =" + ptnumber + ", @material_number=" + material_number
                            + ", sql = " + sql);
                        cmd.Parameters.Add(new SqlParameter("@ptnum", ptnumber));
                        cmd.Parameters.Add(new SqlParameter("@matl_num", material_number));
                        cmd.Parameters.Add(new SqlParameter("@plant", plant));
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                T2ProjectMaterialDao idao = new T2ProjectMaterialDao();
                                idao.MatlId = (Int32)reader["matlId"];
                                idao.ProjectId = (Int32)reader["project_id"];
                                idao.ProjectName = (string)reader["project_name"];
                                idao.PSLFamily = (string)reader["psl_family"];
                                idao.SubPsl = (string)reader["sub_psl"];
                                idao.SingZewo = (string)reader["sing_zewo"];
                                idao.SAPNetwork = (string)reader["sap_network"];
                                idao.ActCode = (string)reader["activity_code"];
                                idao.TechProjMgr = (string)reader["tech_project_mgr"];
                                idao.TechRespEngr = (string)reader["tech_resp_engineer"];                              


                                //add it to ArrayList
                                al.Add(idao);
                            }
                        }
                    }
                }

            }
            catch (Exception ee)
            {
                log.Error("Error in getting getAllt2ProjectAndMaterailsForPlanner() " + ee.Message);
            }

            return al;
        }


        private static String SQL_PROJECT_MATERIAL = "SELECT m.[id] as matlId, m.project_id  ,p.[plant] ,p.[project_name] ,p.[psl_family], p.[sub_psl], p.[sing_zewo], p.[tech_project_mgr]" +
    ",p.[tech_resp_engineer], p.sap_network ,p.[activity_code],   p.[tracking_number], p.[entry_date] ,m.material_num  " +
    ",m.material_rev , m.[total_req_qty]  ,m.material_unit, m.[material_desc],  m.[raw_material] ,m.[material_spec]  ,m.[routing_scope]   " +
    ",m.[drawing_num] ,m.[drawing_revision]  ,m.[comments] ," +
    " p.[need_date]  ,m.[prod_ZEWO]  ,m.[raw_matl_assignment] ,m.[routing_status]  ,m.[router_list] ,m.[program_status]  ,m.[program_list]" +
    " ,m.[tagged_raw_mtrl_ETA]      ,m.[RM_TK_Purch_ETA] ,m.[RMl_TK_and_purch_part_po_ln]  ,m.[RM_TK_Purch_GR]" +
    ",m.[RMl_TK_and_purch_part_Preq]  ,m.[PR_line], m.[ZNDPO] , m.[ZNDPO_line] ,m.[farmout_po_line] ,m.[farmout_eta]" +
    ",m.[farmout_gr_date]      ,m.[farmout_preq]      ,m.[farmout_pr_line] , m.[material_due_date] ,p.[promised_date] , p.[priority] ,m.[DIQ]  " +
    ",m.[PCNF_WIP], m.[unconfirmed_wc],  m.UPD_BY, m.UPD_ON, m.material_status,  m.vendor_name, m.customer_name, p.[tpc_contact] " +
    ", m.wc_del_date, m.wc_owner, m.wc_remarks, m.wc_vendor, p.project_status, p.form_type " +
           " FROM t2_project p with (nolock)  inner join t2_material m with (nolock) on p.id=m.project_id  ";

        public static ArrayList getAllt2ProjectAndMaterails(String plant, String ptnumber, String material_number)
        {
            ArrayList al = new ArrayList();

            try
            {
                //p.plant = @plant and
                  String sql = SQL_PROJECT_MATERIAL + " where  p.tracking_number=@ptnum and  m.material_num=@matl_num";

                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.CommandType = CommandType.Text;
                        log.Info("getAllT2Projects() query : Tracking Num =" + ptnumber + ", @material_number=" + material_number
                            + ", sql = " + sql);
                        cmd.Parameters.Add(new SqlParameter("@ptnum", ptnumber));
                        cmd.Parameters.Add(new SqlParameter("@matl_num", material_number));
                        cmd.Parameters.Add(new SqlParameter("@plant", plant));
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                T2ProjectMaterialDao idao = new T2ProjectMaterialDao();
                                idao.MatlId = (Int32)reader["matlId"];
                                idao.ProjectId = (Int32)reader["project_id"];
                                idao.ProjectName = (string)reader["project_name"];
                                idao.PSLFamily = (string)reader["psl_family"];
                                idao.SubPsl = (string)reader["sub_psl"];
                                idao.SingZewo = (string)reader["sing_zewo"];
                                idao.SAPNetwork = (string)reader["sap_network"];
                                idao.ActCode = (string)reader["activity_code"];
                                idao.TechProjMgr = (string)reader["tech_project_mgr"];
                                idao.TechRespEngr = (string)reader["tech_resp_engineer"];

                                if ((reader["entry_date"]) != DBNull.Value)
                                {
                                    idao.EntryDate = (DateTime)reader["entry_date"];
                                }

                                if (reader["need_date"] != DBNull.Value)
                                {
                                    idao.NeedDate = (DateTime)reader["need_date"];
                                }

                                /* if (reader["start_date"] != DBNull.Value)
                                 {
                                     idao.StartDate = (DateTime)reader["start_date"];
                                 }*/

                                idao.TrackingNumber = (string)reader["tracking_number"];

                                idao.Plant = (string)reader["plant"];
                                idao.ProjectStatus = (string)reader["project_status"];

                                if (reader["form_type"] != DBNull.Value)
                                {
                                    idao.FormType = (string)reader["form_type"];
                                }

                                idao.TPContact = (string)reader["tpc_contact"];

                                if (reader["priority"] != DBNull.Value)
                                {
                                    idao.Priority = (string)reader["priority"];
                                }

                                if (reader["promised_date"] != DBNull.Value)
                                {
                                    idao.PromisedDate = (DateTime)reader["promised_date"];
                                }

                                /*idao.TotalLineItems = (int)reader["total_line_items"];
                                idao.TotalCompleted = (int)reader["total_completed"];
                                idao.CompletedPercentage = (Double)reader["completed_percent"];
                                if (reader["ownership"] != DBNull.Value)
                                {
                                    idao.Ownership = (string)reader["ownership"];
                                }*/


                                if (reader["material_num"] != DBNull.Value)
                                {
                                    idao.MaterialNbr = (string)reader["material_num"];
                                }
                                if (reader["material_rev"] != DBNull.Value)
                                {
                                    idao.MaterialRev = (string)reader["material_rev"];
                                }

                                if (reader["total_req_qty"] != DBNull.Value)
                                {
                                    idao.TotalReqQty = (int)reader["total_req_qty"];
                                }
                                if (reader["material_unit"] != DBNull.Value)
                                {
                                    idao.MaterialUnit = (string)reader["material_unit"];
                                }

                                if (reader["material_desc"] != DBNull.Value)
                                {
                                    idao.MaterialDesc = (string)reader["material_desc"];
                                }

                                if (reader["raw_material"] != DBNull.Value)
                                {
                                    idao.RawMaterial = (string)reader["raw_material"];
                                }

                                if (reader["material_spec"] != DBNull.Value)
                                {
                                    idao.MaterialSpec = (string)reader["material_spec"];
                                }

                                if (reader["routing_scope"] != DBNull.Value)
                                {
                                    idao.RoutingScope = (string)reader["routing_scope"];
                                }
                                if (reader["drawing_num"] != DBNull.Value)
                                {
                                    idao.DrawingNbr = (string)reader["drawing_num"];
                                }
                                if (reader["drawing_revision"] != DBNull.Value)
                                {
                                    idao.DrawingRev = (string)reader["drawing_revision"];
                                }
                                if (reader["comments"] != DBNull.Value)
                                {
                                    idao.Comments = (string)reader["comments"];
                                }
                                if (reader["prod_ZEWO"] != DBNull.Value)
                                {
                                    idao.ProdZEWO = (string)reader["prod_ZEWO"];
                                }
                                if (reader["raw_matl_assignment"] != DBNull.Value)
                                {
                                    idao.RawMaterialAssmnt = (string)reader["raw_matl_assignment"];
                                }
                                if (reader["routing_status"] != DBNull.Value)
                                {
                                    idao.RoutingStatus = (string)reader["routing_status"];
                                }
                                if (reader["router_list"] != DBNull.Value)
                                {
                                    idao.Routers = (string)reader["router_list"];
                                }
                                if (reader["program_status"] != DBNull.Value)
                                {
                                    idao.ProgramStatus = (string)reader["program_status"];
                                }
                                if (reader["program_list"] != DBNull.Value)
                                {
                                    idao.ProgramsList = (string)reader["program_list"];
                                }
                                if (reader["tagged_raw_mtrl_ETA"] != DBNull.Value)
                                {
                                    idao.RawMaterialETA = (string)reader["tagged_raw_mtrl_ETA"];
                                }
                                if (reader["RM_TK_Purch_ETA"] != DBNull.Value)
                                {
                                    idao.RMTKPUR_ETA = (DateTime)reader["RM_TK_Purch_ETA"];
                                }
                                if (reader["RMl_TK_and_purch_part_po_ln"] != DBNull.Value)
                                {
                                    idao.RMTKPUR_POLN = (string)reader["RMl_TK_and_purch_part_po_ln"];
                                }

                                if (reader["RM_TK_Purch_GR"] != DBNull.Value)
                                {
                                    idao.RMTKPUR_GR_Date = (DateTime)reader["RM_TK_Purch_GR"];
                                }
                                if (reader["RMl_TK_and_purch_part_Preq"] != DBNull.Value)
                                {
                                    idao.RMTKPUR_PReq = (string)reader["RMl_TK_and_purch_part_Preq"];
                                }
                                if (reader["PR_line"] != DBNull.Value)
                                {
                                    idao.PR_LN_NBR = (int)reader["PR_line"];
                                }

                                if (reader["ZNDPO"] != DBNull.Value)
                                {
                                    idao.ZNDPO = (string)reader["ZNDPO"];
                                }
                                if (reader["ZNDPO_line"] != DBNull.Value)
                                {
                                    idao.ZNDPO_LN = (int)reader["ZNDPO_line"];
                                }
                                if (reader["farmout_po_line"] != DBNull.Value)
                                {
                                    idao.Farmout_PO_LN = (string)reader["farmout_po_line"];
                                }

                                if (reader["farmout_eta"] != DBNull.Value)
                                {
                                    idao.Farmout_ETA = (DateTime)reader["farmout_eta"];
                                }
                                if (reader["farmout_gr_date"] != DBNull.Value)
                                {
                                    idao.Farmout_GR_Date = (DateTime)reader["farmout_gr_date"];
                                }
                                if (reader["farmout_preq"] != DBNull.Value)
                                {
                                    idao.Farmout_PReq = (string)reader["farmout_preq"];
                                }

                                if (reader["farmout_pr_line"] != DBNull.Value)
                                {
                                    idao.Farmout_PR_LN = (int)reader["farmout_pr_line"];
                                }
                                if (reader["material_due_date"] != DBNull.Value)
                                {
                                    idao.MaterialDueDate = (DateTime)reader["material_due_date"];
                                }
                                if (reader["DIQ"] != DBNull.Value)
                                {
                                    idao.Diq = (int)reader["DIQ"];
                                }

                                if (reader["PCNF_WIP"] != DBNull.Value)
                                {
                                    idao.PCNF = (string)reader["PCNF_WIP"];
                                }
                                if (reader["unconfirmed_wc"] != DBNull.Value)
                                {
                                    idao.UNConfirmWC = (string)reader["unconfirmed_wc"];
                                }
                                if (reader["material_status"] != DBNull.Value)
                                {
                                    idao.MaterialStatus = (string)reader["material_status"];
                                }

                                if (reader["vendor_name"] != DBNull.Value)
                                {
                                    idao.VendorName = (string)reader["vendor_name"];
                                }
                                if (reader["customer_name"] != DBNull.Value)
                                {
                                    idao.BuyerName = (string)reader["customer_name"];
                                }

                                if (reader["tpc_contact"] != DBNull.Value)
                                {
                                    idao.TPContact = (string)reader["tpc_contact"];
                                }
                                if (reader["wc_del_date"] != DBNull.Value)
                                {
                                    idao.WCDeliveryDate = (DateTime)reader["wc_del_date"];
                                }

                                if (reader["wc_owner"] != DBNull.Value)
                                {
                                    idao.WCOwner = (string)reader["wc_owner"];
                                }
                                if (reader["wc_remarks"] != DBNull.Value)
                                {
                                    idao.WCRemarks = (string)reader["wc_remarks"];
                                }
                                if (reader["wc_vendor"] != DBNull.Value)
                                {
                                    idao.WCVendor = (string)reader["wc_vendor"];
                                }


                                //add it to ArrayList
                                al.Add(idao);
                            }
                        }
                    }
                }

            }
            catch (Exception ee)
            {
                log.Error("Error in getting getAllt2ProjectAndMaterails() " + ee.Message);
            }
            return al;
        }



    }
}
