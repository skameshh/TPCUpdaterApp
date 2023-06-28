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
 * its loaded in ;'PlannerTrigger'
 * 
 * based one prod zewo,  there may be chance that one prod zewo linked to many material.
 */
namespace TPC2UpdaterApp.Helpers
{
    public class ZEWOStatusUpdate4Planner
    {
        /**
         * 1. Get all in-progress status
         * 2. update all status in t2_material table with date 
         * 
         */

        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ZEWOStatusUpdate4Planner()
        {

        }

        public static void doUpdateZewostatusNewDischard()
        {
            try
            {
                ArrayList zewo_list = getAllTPCMaterials();

                if (zewo_list.Count>0)
                {
                    ArrayList closedList = getAllProdOrderStatusFromHCTDB(zewo_list);
                    if (closedList.Count>0)
                    {
                        //update this order in t2_material;
                        updateT2Material(closedList);
                    }
                }                
                
            }catch(Exception ee)
            {
                log.Info(" " + ee.Message);
            }
        }

        private static void updateT2Material(ArrayList clist)
        {
            ArrayList al = new ArrayList();
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();

                    for (int x = 0; x < clist.Count; x++) {
                        string zewo = (string)clist[x];
                        string sql = "update t2_material set material_status=\'Completed\', closed_on_from_sap='"+ today + "' where prod_zewo='"+ zewo.Trim() + "'";
                        log.Info("updateT2Material() query : sql = "+sql);

                        using (SqlCommand cmd = new SqlCommand(sql, cnn))
                        {
                            cmd.CommandType = CommandType.Text;                          
                            //cmd.Parameters.Add(new SqlParameter("@ACTION", action));

                            cmd.ExecuteNonQuery();
                            log.Info("updateT2Material()  zewo = " + zewo);
                        }
                    }
                }

                log.Info("======= updateT2Material()   completed =========\n\n " );
            }
            catch (Exception ee)
            {
                log.Error("Error in getting updateT2Material() " + ee.Message);
            }


            updatedHistory(clist);
        }


        private static void updatedHistory(ArrayList clist)
        {
            for (int x = 0; x < clist.Count; x++)
            {
                string zewo = (string)clist[x];

                DBUtils.insertUsageHistoryHelper(MYGlobal.ACT_UPDATOR_LINE_TO_COMPLETE, "HID:System"  
                    + ", " + MYGlobal.MSG_MASTER_LIST_UPDATE_STATUS + ", ZEWO:" + zewo  );
            }

        }


        private static ArrayList getAllProdOrderStatusFromHCTDB(ArrayList matList)
        {
            ArrayList al = new ArrayList();
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getHCTDBCString()))
                {
                    cnn.Open();

                    for (int x = 0; x < matList.Count; x++)
                    {
                        string ord = (string)matList[x];
                        string sql = "select  * from production_order_status  WITH (NOLOCK) where ORDER_NUM='000" + ord + "'";
                        // log.Info("getAllProdOrderStatusFromHCTDB() query : sql = "+sql);


                        using (SqlCommand cmd = new SqlCommand(sql, cnn))
                        {
                            cmd.CommandType = CommandType.Text;

                            //cmd.Parameters.Add(new SqlParameter("@ACTION", action));

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    bool is_closed = false;

                                    if ((reader["DELIVERED_IND"]) != DBNull.Value)
                                    {
                                        string delv = (String)reader["DELIVERED_IND"];
                                        if (delv.Equals("X") || delv.Equals("x"))
                                        {
                                            is_closed = true;
                                        }
                                    }

                                    if ((reader["TECO_IND"]) != DBNull.Value)
                                    {
                                        string delv = (String)reader["TECO_IND"];
                                        if (delv.Equals("X") || delv.Equals("x"))
                                        {
                                            is_closed = true;
                                        }
                                    }

                                    if (is_closed)
                                    {
                                        log.Info("getAllProdOrderStatusFromHCTDB() closed order = " + ord);
                                        al.Add(ord);//add whichever is closed
                                    }
                                }
                            }
                        }//cmd
                    }//for
                }
            }
            catch (Exception ee)
            {
                log.Error("Error in getting getAllProdOrderStatusFromHCTDB() " + ee.Message);
            }
            return al;
        }

        private static ArrayList getAllProdOrderStatusFromHCTDBOld(ArrayList matList)
        {
            ArrayList al = new ArrayList();
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getHCTDBCString() ))
                {
                    cnn.Open();

                    for (int x = 0; x < matList.Count; x++)
                    {
                        string ord = (string)matList[x];
                        string sql = "select  * from production_order_status  WITH (NOLOCK) where ORDER_NUM='000" + ord + "'";
                       // log.Info("getAllProdOrderStatusFromHCTDB() query : sql = "+sql);


                        using (SqlCommand cmd = new SqlCommand(sql, cnn))
                        {
                            cmd.CommandType = CommandType.Text;
                            
                            //cmd.Parameters.Add(new SqlParameter("@ACTION", action));

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    bool is_closed = false;
                                   
                                    if ((reader["DELIVERED_IND"]) != DBNull.Value)
                                    {
                                       string  delv = (String)reader["DELIVERED_IND"];
                                        if (delv.Equals("X") || delv.Equals("x"))
                                        {
                                            is_closed = true;
                                        }
                                    }

                                    if ((reader["TECO_IND"]) != DBNull.Value)
                                    {
                                        string delv = (String)reader["TECO_IND"];
                                        if (delv.Equals("X") || delv.Equals("x"))
                                        {
                                            is_closed = true;
                                        }
                                    }

                                    if (is_closed)
                                    {
                                        log.Info("getAllProdOrderStatusFromHCTDB() closed order = "+ord);
                                        al.Add(ord);//add whichever is closed
                                    }   
                                }
                            }
                        }//cmd
                    }//for
                }
            }
            catch (Exception ee)
            {
                log.Error("Error in getting getAllProdOrderStatusFromHCTDB() " + ee.Message);
            }
            return al;
        }


        /*private static String MATL_SQL_old = "select  distinct(prod_zewo) from t2_material  WITH (NOLOCK) " +
            " where material_status='In-Progress' and len(prod_zewo)>0";*/

        private static String MATL_SQL = "select  distinct(prod_zewo),tracking_number from view_t2_project_material  WITH (NOLOCK) " +
           " where material_status='In-Progress' and len(prod_zewo)>0";

        private static ArrayList getAllTPCMaterials()
        {
            ArrayList al = new ArrayList();
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(MATL_SQL, cnn))
                    {
                        cmd.CommandType = CommandType.Text;
                        log.Info("getAllTPCMaterials() query : MATL_SQL");
                        //cmd.Parameters.Add(new SqlParameter("@ACTION", action));

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                String prod_zeo = "";
                                if ((reader["prod_ZEWO"]) != DBNull.Value)
                                {
                                    prod_zeo = (String)reader["prod_ZEWO"];
                                    al.Add(prod_zeo);
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                log.Error("Error in getting getAllTPCMaterials() " + ee.Message);
            }
            return al;
        }


    }
}
