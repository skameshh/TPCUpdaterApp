using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TPC2UpdaterApp.DB;

/**
 * This using in GlobalReportHelper
 * 
 */
namespace TPC2UpdaterApp.Helpers
{
    class PlannerAppoxiEstmateDate
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);



        public static void doUpdateApproxPromixedDate()
        {
            log.Info("doUpdateApproxPromixedDate");

            //Load all TPC WC
            getAllTPCWC();

            ArrayList matllist =  getAllT2MaterialsForPromizedDate();
            for (int x = 0; x < matllist.Count; x++)
            {
                MBMaterialDao dao = (MBMaterialDao)matllist[x];
                log.Info("doUpdateApproxPromixedDate()  unconfirm = " + dao.WCUnconfirmOper +", id="+dao.MatlId +"\n\n");

                String wc = dao.WCUnconfirmOper;
                if (wc.Length < 8)
                {
                    doUpdateMaterialTable(dao.MatlId, wc);
                    continue;
                }

                int days = 0;
                try
                {
                    string[] all_wc = (string[])wc.Split(' ');
                    
                    for (int y = 0; y < all_wc.Length; y++)
                    {
                        string one_wc = (string)all_wc[y];
                        if (one_wc.Length > 5)
                        {
                            int one = (int)map[one_wc];
                            days = days + one;
                        }
                    }
                }catch(Exception ee)
                {
                    log.Info("doUpdateApproxPromixedDate Error " + ee.Message);
                    continue;
                }

                if (days > 0)
                {
                    DateTime routing_sts_date = dao.RoutingStsCompltdDate;
                    DateTime after_add_days = routing_sts_date.AddDays(days);

                    log.Info("doUpdateApproxPromixedDate Total days for this WC " + days + ", routing_sts_date=" + routing_sts_date + ", after_add_days=" + after_add_days.ToString("yyyy-MM-dd") + "\n\n");
                    Thread.Sleep(200);
                    doUpdateMaterialTable(dao.MatlId, after_add_days.ToString("yyyy-MMM-dd"));
                    Thread.Sleep(200);
                }
            }//for


            doPurchaseUpdate();
        }

        public static void doPurchaseUpdate()
        {
            doPurchaseServiceUpdate("Purchase");
            Thread.Sleep(2000);
            doPurchaseServiceUpdate("PurchaseRFQ");
            Thread.Sleep(2000);
            doPurchaseServiceUpdate("Service");
        }


        private static void doPurchaseServiceUpdate(string type)
        {
            try
            {
                ArrayList matllist = getAllT2MaterialsForPurcase_Service(type);
                for (int x = 0; x < matllist.Count; x++)
                {
                    MBMaterialDao dao = (MBMaterialDao)matllist[x];
                    log.Info("doPurchaseServiceUpdate() id=" + dao.MatlId + "\n\n");

                    int days = 70;
                    if (type.Equals("Service"))
                    {
                        days = 14;
                    }
                    if (days > 0)
                    {
                        DateTime routing_sts_date = dao.RoutingStsCompltdDate;
                        DateTime after_add_days = routing_sts_date.AddDays(days);

                        log.Info("doPurchaseServiceUpdate Total days for this WC " + days + ", routing_sts_date=" + routing_sts_date + ", after_add_days=" + after_add_days.ToString("yyyy-MM-dd") + "\n\n");
                        Thread.Sleep(200);
                        doUpdateMaterialTable(dao.MatlId, after_add_days.ToString("yyyy-MMM-dd"));
                        Thread.Sleep(200);
                    }
                }
            }catch(Exception ee)
            {
                log.Error("EE " + ee.Message);
            }
        }


        private static void doUpdateMaterialTable(int id, string approx_date)
        {
            string sql = "update t2_material set desc_2='" + approx_date + "' where id=" + id;
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.CommandType = CommandType.Text;
                        log.Info(" ApproxPromixedDate doUpdateMaterialTable()  sql = " + sql);

                        int affRows = cmd.ExecuteNonQuery();
                        log.Info("Affrows = " + affRows);
                    }
                }
            }
            catch (Exception ee)
            {

                log.Error("Error in ApproxPromixedDate doUpdateMaterialTable() = " + ee.Message + "\n\n");
            }
        }



       static Dictionary<String , int> map = new Dictionary<String, int>();

        private static void getAllTPCWC()
        {
            
            string sql = "select id, tpc_work_centre, approx_days from t_tpc_dnc_wc WITH (NOLOCK)  ";
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.CommandType = CommandType.Text;
                        log.Info("ApproxPromixedDate getTPCWC() sql=" + sql);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                 
                                int id = (Int32)reader["Id"];
                                string tpc_work_centre = (String)reader["tpc_work_centre"];
                                int days = 0;
                                if ((reader["approx_days"]) != DBNull.Value)
                                {
                                    days = (int)reader["approx_days"];
                                }

                                    map.Add(tpc_work_centre.Trim(), days);
                            }
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                log.Error("Error in getting ApproxPromixedDate getAllTPCWC() " + ee.Message);
            }
             
        }


        private static ArrayList getAllT2MaterialsForPromizedDate()
        {
            ArrayList al = new ArrayList();
            string sql = "select  id, wc_unconfirm_wc, material_num,material_status, routing_status, routing_sts_completed_date " +
                " from t2_material WITH (NOLOCK) " +
                " where material_status='In-Progress' and routing_status='Completed' and len(wc_unconfirm_wc)>0 " +
                " and wc_promised_date is null ";//and desc_2 is null - cant update because it will updated everyday
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.CommandType = CommandType.Text;
                        log.Info("getAllT2MaterialsForPromizedDate() sql=" + sql);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                MBMaterialDao dao = new MBMaterialDao();
                                dao.MatlId = (Int32)reader["Id"];
                                dao.MaterialNum = (String)reader["material_num"];
                                dao.MaterialStatus = (String)reader["material_status"];
                                dao.RoutingSts = (String)reader["routing_status"];

                                //

                                if ((reader["wc_unconfirm_wc"]) != DBNull.Value)
                                {
                                    dao.WCUnconfirmOper = (String)reader["wc_unconfirm_wc"];
                                    log.Info("WCUnconfirmOper = " + dao.WCUnconfirmOper);
                                }

                                if ((reader["routing_sts_completed_date"]) != DBNull.Value)
                                {
                                    dao.RoutingStsCompltdDate = (DateTime)reader["routing_sts_completed_date"];
                                    log.Info("RoutingStsCompltdDate = " + dao.RoutingStsCompltdDate);
                                }

                                al.Add(dao);
                            }
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                log.Error("Error in getting getAllT2MaterialsForPromizedDate() " + ee.Message);
            }
            return al;
        }




        private static ArrayList getAllT2MaterialsForPurcase_Service(string type)
        {
            ArrayList al = new ArrayList();

            string sql = string.Empty;
            string routing_scope = string.Empty;
            if (type.Equals("Purchase"))
            {
                routing_scope = "Purchase";
            }
            else if (type.Equals("PurchaseRFQ"))
            {
                routing_scope = "Purch RFQ";
            }
            else if (type.Equals("Service"))
            {
                routing_scope = "Service";
            }

            sql = "select  id, material_num, material_status, routing_status, routing_sts_completed_date " +
               " from t2_material WITH (NOLOCK) " +
               " where material_status='In-Progress' and routing_status='Completed' and  routing_scope='"+ routing_scope + "' " +
               " and wc_promised_date is null ";


            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.CommandType = CommandType.Text;
                        log.Info("getAllT2MaterialsForPurcase_Service() sql=" + sql);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                MBMaterialDao dao = new MBMaterialDao();
                                dao.MatlId = (Int32)reader["Id"];
                                dao.MaterialNum = (String)reader["material_num"];
                                dao.MaterialStatus = (String)reader["material_status"];
                                dao.RoutingSts = (String)reader["routing_status"];
                                
                                if ((reader["routing_sts_completed_date"]) != DBNull.Value)
                                {
                                    dao.RoutingStsCompltdDate = (DateTime)reader["routing_sts_completed_date"];
                                    log.Info("RoutingStsCompltdDate = " + dao.RoutingStsCompltdDate);
                                }

                                al.Add(dao);
                            }
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                log.Error("Error in getting getAllT2MaterialsForPurcase_Service() " + ee.Message);
            }
            return al;
        }


    }
}
