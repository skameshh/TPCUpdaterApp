using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPC2UpdaterApp.DB;

/***
 * Triggered by QCTrigger
 * 
 */
namespace TPC2UpdaterApp.Forms
{
   public class QCHelper
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /**
         * 1. Get all in-progress materials and prod_zewo
         * 2. Get all prod_zewo from t_unsch
         * 3. check prod_zewo and operation available in t2_qc
         * 4. if not insert.
         * 
         */
        public static void doQCUpdate()
        {
            try
            {
                doQC();

              
            }catch(Exception ee)
            {
                log.Error("Error doQCUpdate() " + ee.Message);
            }

            try
            {
                
                //working on it
                TurnkeyFarmoutHelper.doUpdateFarmoutTurnkey();
            }
            catch (Exception ee)
            {
                log.Error("Error doQCUpdate() " + ee.Message);
            }


        }

        private static void doQC()
        {
            ArrayList al = getAllTPCMaterials();
            if (al != null)
            {
                for (int x = 0; x < al.Count; x++)//al.Count
                {
                    String prozewo = (String)al[x];
                    ArrayList alSch = getAllUnSch(prozewo);
                    for (int i = 0; i < alSch.Count; i++)
                    {
                        UnSchDao dao = (UnSchDao)alSch[i];
                        doInsertT2Operations(dao, prozewo);
                    }

                }
            }
        }

        private static String MATL_SQL = "select  distinct(prod_zewo) from t2_material  WITH (NOLOCK) where material_status='In-Progress' and len(prod_zewo)>0";

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


        private static String SQL_UNSCH_SQL = "";// "select  * from t_me2n where pr_num =@PR_NUM  and pr_item_num=@PR_ITEM_NUM;";
        private static ArrayList getAllUnSch(String prodzewo)
        {

            SQL_UNSCH_SQL = "select  * from t_unsch  WITH (NOLOCK) where mfg_order like '%" + prodzewo + "%'";

            ArrayList al = new ArrayList();
            using (SqlConnection con = new SqlConnection(MYGlobal.getCString()))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(SQL_UNSCH_SQL, con))
                {
                    cmd.CommandType = CommandType.Text;
                    // cmd.Parameters.AddWithValue("@PR_NUM", "00"+prnum);
                    //cmd.Parameters.AddWithValue("@PR_ITEM_NUM", pritemnum);

                    log.Info("  Unsch sql =" + SQL_UNSCH_SQL);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            try
                            {
                                UnSchDao dao = new UnSchDao();
                                dao.Plant = (string)reader["PLANT"];
                                // dao.Id = (Int32)reader["id"];

                                if (reader["UnConfirm_WorkCenter"] != DBNull.Value)
                                {
                                    //if no Po-num dont add it
                                    dao.UnConfirmWC = (String)reader["UnConfirm_WorkCenter"];
                                    log.Info("prodzewo:" + prodzewo + "  >  Got UnConfirmWC : " + dao.UnConfirmWC);
                                }
                                else
                                {
                                    continue;
                                }

                                if (reader["oper_id"] != DBNull.Value)
                                {
                                    dao.OperationId = (String)reader["oper_id"];
                                }
                                else
                                {
                                    continue;
                                }

                                if (reader["current_oper"] != DBNull.Value)
                                {
                                    dao.CurrentOper = (Int32)reader["current_oper"];
                                }


                                if (reader["oper"] != DBNull.Value)
                                {
                                    dao.Oper = (Int32)reader["oper"];
                                }

                                if (reader["Order_Qty"] != DBNull.Value)
                                {
                                    dao.Qty = (int)reader["Order_Qty"];
                                }


                                if (reader["resource"] != DBNull.Value)
                                {
                                    dao.ResourceWC = (String)reader["resource"];
                                }

                                if (reader["resource_descrition"] != DBNull.Value)
                                {
                                    dao.ResourceDesc = (String)reader["resource_descrition"];
                                }

                                if (reader["Run_time_Hrs"] != DBNull.Value)
                                {
                                    dao.RunTimeHrs = (double)reader["Run_time_Hrs"];
                                }



                                //----- add ---
                                al.Add(dao);

                            }
                            catch (Exception ee)
                            {
                                log.Error("Error in getAllUnSch : " + ee.Message);
                            }


                        }
                    }
                }
            }

            return al;
        }



        public static void doInsertT2Operations(UnSchDao dao, String prod_zewo)
        {
            int affRows = 0;

            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();

                    using (SqlCommand cmd = new SqlCommand("[SP_INSERT_UPDATE_QC_OPERATIONS]", cnn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (dao.Id > 0)
                        {
                            cmd.Parameters.AddWithValue("@ID", dao.Id);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@ID", 0);
                        }

                        if (dao.ResourceWC.Contains("QC"))
                        {
                            cmd.Parameters.AddWithValue("@PROD_ZEWO", prod_zewo);
                            cmd.Parameters.AddWithValue("@OPERATION_NUM", dao.Oper);

                            cmd.Parameters.AddWithValue("@CURRENT_OPR", dao.CurrentOper);
                            cmd.Parameters.AddWithValue("@OPERATION", dao.OperationId);
                            cmd.Parameters.AddWithValue("@RESOURCE_WC", dao.ResourceWC);
                            cmd.Parameters.AddWithValue("@RESOURCE_WC_DESC", dao.ResourceDesc);
                            cmd.Parameters.AddWithValue("@ORD_QTY", dao.Qty);
                            cmd.Parameters.AddWithValue("@UNCONFIRM_WC", dao.UnConfirmWC);
                            cmd.Parameters.AddWithValue("@RUN_TIME_HRS", dao.RunTimeHrs);
                            cmd.Parameters.AddWithValue("@QC_STATUS", "New");
                            cmd.Parameters.AddWithValue("@INSERT_DATE", DateTime.Now.ToString("yyyy-MM-dd"));


                            //exe
                            affRows = cmd.ExecuteNonQuery();
                            log.Info(" Insert QC Success, Affrows = " + affRows);
                        }
                        else
                        {
                            log.Info("Not a QC Operation "+ dao.ResourceWC);
                        }

                       

                    }
                }
            }
            catch(Exception ee)
            {
                log.Info("Error in saving QC=> " + ee.Message);
            }
        }
         
    }
}
