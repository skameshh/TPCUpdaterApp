using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
        select top 10 * from t_zm33 where prod_ordr_num='106769728';

        --3 update
     * 
     */
    class ZM33Trigger
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void doUpdateZM33()
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
                            ZM33Dao zmdao = (ZM33Dao)mal[xx];
                            //3. Update t2_material
                            bool bb = doUpdatet2MaterialforZm33(zmdao, dao.Id);
                            if (bb)
                            {
                                log.Info("update unSch success");
                            }
                            else
                            {
                                log.Info("update unSch failed");
                            }
                        }
                    }
                }
            }

            log.Info("============UnSch Completed ==========\n\n\n");
        }


        private static String SQL_UPDATE_T2_Material_UNSch = "update t2_material set system_status_updated=@sysstatus, DIQ=@diq " +
          "   where id=@id";


        private static bool doUpdatet2MaterialforZm33(ZM33Dao zmdao, int id)
        {
            ArrayList al = new ArrayList();
            using (SqlConnection con = new SqlConnection(MYGlobal.getCString()))
            {
                con.Open();
                String sql = "";

                sql = SQL_UPDATE_T2_Material_UNSch;

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    try
                    {

                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@sysstatus", zmdao.WorkCentre);
                        cmd.Parameters.AddWithValue("@diq", zmdao.DaysInQueue);                         

                        //where
                        cmd.Parameters.AddWithValue("@id", id);

                        log.Info("doUpdatet2MaterialforZm33() t2_material update sql=" + sql + ", material_id=" + id + ", system_status_updated " + zmdao.WorkCentre +", DIQ="+zmdao.DaysInQueue);

                        int tt = cmd.ExecuteNonQuery();
                        log.Info("Update material for zm33 result : " + tt);
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



        private static String SQL_ZM33_SQL = "";// "select  * from t_me2n where pr_num =@PR_NUM  and pr_item_num=@PR_ITEM_NUM;";
        private static ArrayList getAllUnSch(String prodzewo)
        {

            SQL_ZM33_SQL = "select  * from t_zm33  WITH (NOLOCK) where prod_ordr_num like '%" + prodzewo + "%'";

            ArrayList al = new ArrayList();
            using (SqlConnection con = new SqlConnection(MYGlobal.getCString()))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(SQL_ZM33_SQL, con))
                {
                    cmd.CommandType = CommandType.Text;
                    // cmd.Parameters.AddWithValue("@PR_NUM", "00"+prnum);
                    //cmd.Parameters.AddWithValue("@PR_ITEM_NUM", pritemnum);

                    log.Info("  SQL_ZM33_SQL   =" + SQL_ZM33_SQL);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            try
                            {
                                ZM33Dao dao = new ZM33Dao();
                                dao.Plant = (string)reader["plant_id"];
                                // dao.Id = (Int32)reader["id"];

                                if (reader["work_centre"] != DBNull.Value)
                                {
                                    //if no Po-num dont add it
                                    dao.WorkCentre = (String)reader["work_centre"];
                                    log.Info("Got WorkCentre : " + dao.WorkCentre);
                                }
                                else
                                {
                                    continue;
                                }


                                if (reader["oper_num"] != DBNull.Value)
                                {
                                    dao.CurrentOper = (Int32)reader["oper_num"];
                                }


                                if (reader["oper_desc"] != DBNull.Value)
                                {
                                    dao.CurrentOperDesc = (String)reader["oper_desc"];
                                }

                                if (reader["inactivity_days"] != DBNull.Value)
                                {
                                    dao.DaysInQueue = (int)reader["inactivity_days"];
                                }

                                //add to list
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



        private static String SQL_T2_MATERIAL_IN_PROGRESS_RMTKPRQ = "select  * from t2_material  WITH (NOLOCK) where material_status='In-Progress' and len(prod_ZEWO)>0 ";
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
