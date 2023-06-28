using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPC2UpdaterApp.DB;

namespace TPC2UpdaterApp.Trigger
{
    /**
     * --1
        select  * from t2_material where material_status='In-Progress' and len(prod_ZEWO)>0;

        select  * from t2_material where material_status='In-Progress' and  prod_ZEWO='106769728';

        --2
        select * from t_unsch where mfg_order like '%106769728%';--ITEM-000106769728

        --3
        Update
     * 
     */
    class UNschTrigger
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        //If material status is completed delete unconf_wc and pcnf days 

        public static void doUpdateUnSch()
        {
            log.Info("============SCH Started ==========\n\n\n");

            //1. Fetch all In-Progress
            ArrayList al = getAllInProgressMaterial();
            if (al.Count > 0)
            {
                for (int x = 0; x < al.Count; x++)
                {
                    T2MaterialDao dao = (T2MaterialDao)al[x];
                    //2. Fetch All Unsch
                    ArrayList mal = getAllUnSch(dao.ProdZEWO);
                    if (mal.Count > 0)
                    {
                        //Update t2_material;
                        for (int xx = 0; xx < mal.Count; xx++)
                        {
                            UnSchDao unSchDao = (UnSchDao)mal[xx];
                            //3. Update t2_material

                            try
                            {
                                log.Info("d = ["+ unSchDao.MPMPCNFDate.Trim()+"]");
                                 
                                    bool bb = doUpdatet2MaterialforSch(unSchDao, dao.Id);
                                    if (bb)
                                    {
                                        log.Info("update unSch success");
                                    }
                                    else
                                    {
                                        log.Info("update unSch failed");
                                    }
                                 
                            }catch(Exception ee)
                            {
                                log.Info("Error : " + ee.Message);
                            }

                               
                        }
                    }
                }
            }

            log.Info("============UnSch Completed ==========\n\n\n");
        }


        private static String SQL_UPDATE_T2_Material_UNSch = "update t2_material set unconfirmed_wc=@unc " +
          "   where id=@id";

        private static String SQL_UPDATE_T2_Material_UNSch_with_Pcnf = "update t2_material set unconfirmed_wc=@unc, PCNF_WIP=@pcnf " +
          "   where id=@id";

        private static bool doUpdatet2MaterialforSch(UnSchDao unSchDao, int id)
        {
            log.Info("Doing  doUpdatet2MaterialforSch()");
            ArrayList al = new ArrayList();
            using (SqlConnection con = new SqlConnection(MYGlobal.getCString()))
            {
                con.Open();
                String sql = "";

                if (unSchDao.MPMPCNFDate.Length>0)
                {
                    sql = SQL_UPDATE_T2_Material_UNSch_with_Pcnf;
                }
                 else
                {
                    sql = SQL_UPDATE_T2_Material_UNSch;
                    log.Info("Update no PCNF date found");
                    //return false;
                } 
                    

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    try
                    {

                        log.Info("Update Unsch prod_zewo : "+ unSchDao.MfgOrder +"  for  Material ID =  "+ id +", date=" + unSchDao.MPMPCNFDate);

                       cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@unc", unSchDao.UnConfirmWC);
                        if (unSchDao.MPMPCNFDate.Length>0)
                        {
                            try
                            {
                                String strdt = unSchDao.MPMPCNFDate;
                                DateTime dtx = DateTime.ParseExact(strdt, "MM/dd/yyyy", CultureInfo.InvariantCulture);

                                /*String mmx = dtx.ToString("yyyy-dd-MM");
                                log.Info("Dtx " + mmx);
                                DateTime mmdd = DateTime.Parse(mmx);*/

                                TimeSpan dff = DateTime.Now.Subtract(dtx);

                                log.Info(DateTime.Now + "-" + dtx.ToString() +", days=" + dff.Days);
                                cmd.Parameters.AddWithValue("@pcnf", dff.Days);
                            }catch(Exception ee)
                            {
                                log.Error("Error = "+ee.Message);
                            }
                        }
                                               
                        //where
                        cmd.Parameters.AddWithValue("@id", id);

                        log.Info("t2_material update sql=" + sql + ", material_id=" + id + ", unconfirmed_wc " + unSchDao.UnConfirmWC +", pcnf date="+ unSchDao.MPMPCNFDate);

                        int tt = cmd.ExecuteNonQuery();
                        log.Info("Update material for SCH result : " + tt);
                        return true;
                    }
                    catch (Exception ee)
                    {
                        log.Info("Update material failed Error : " + ee.Message);
                        return false;
                    }
                }
            }
        }



        private static String SQL_UNSCH_SQL = "";// "select  * from t_me2n where pr_num =@PR_NUM  and pr_item_num=@PR_ITEM_NUM;";
        private static ArrayList getAllUnSch(String prodzewo)
        {

            SQL_UNSCH_SQL = "select  * from t_unsch WITH (NOLOCK)  " +
                " where mfg_order like '%" + prodzewo + "%'"  ;

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
                                    log.Info("prodzewo:"+ prodzewo + "  >  Got UnConfirmWC : " + dao.UnConfirmWC );
                                }
                                else
                                {
                                    continue;
                                }

                                if (reader["MPM_PCNF_Date"] != DBNull.Value)
                                {
                                    dao.MPMPCNFDate = (String)reader["MPM_PCNF_Date"];
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

                                if (reader["Days_in_Queue"] != DBNull.Value)
                                {
                                    dao.DaysInQueue = (int)reader["Days_in_Queue"];
                                }

                                //----- add ---
                                al.Add(dao);

                            }
                            catch (Exception ee)
                            {
                                log.Error("Error in FO getAllUnSch : " + ee.Message);
                            }


                        }
                    }
                }
            }

            return al;
        }



        private static String SQL_T2_MATERIAL_IN_PROGRESS_RMTKPRQ = "select top 100 * from t2_material WITH (NOLOCK)  " +
            " where material_status='In-Progress' and len(prod_ZEWO)>0 ";
        private static ArrayList getAllInProgressMaterial()
        {
            ArrayList al = new ArrayList();
            using (SqlConnection con = new SqlConnection(MYGlobal.getCString()))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(SQL_T2_MATERIAL_IN_PROGRESS_RMTKPRQ, con))
                {
                    cmd.CommandType = CommandType.Text;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            T2MaterialDao dao = new T2MaterialDao();
                            dao.Id = (Int32)reader["id"];
                            dao.ProjectId = (Int32)reader["project_id"];

                            dao.ProdZEWO = (String)reader["prod_ZEWO"];                            

                            log.Info("Got material for UNSch " + dao.Id);
                            al.Add(dao);
                        }
                    }
                }
            }

            return al;
        }

    }
}
