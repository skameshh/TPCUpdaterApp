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
    class FOTrigger
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void doFOUpdate()
        {
            log.Info("============FO Started ==========\n\n\n");

            //1. Fetch all In-Progress
            ArrayList al = getAllInProgressMaterial();
            if (al.Count > 0)
            {
                for (int x = 0; x < al.Count; x++)
                {
                    T2MaterialDao dao = (T2MaterialDao)al[x];
                    //2. Fetch All ME2N
                    ArrayList mal = getAllMe2N4PR(dao.Farmout_PReq, dao.Farmout_PR_LN);
                    if (mal.Count > 0)
                    {
                        //Update t2_material;
                        for (int xx = 0; xx < mal.Count; xx++)
                        {
                            ME2NDao me2ndao = (ME2NDao)mal[xx];
                            //3. Update t2_material
                            bool bb = doUpdatet2MaterialforFO(me2ndao, dao.Id);
                            if (bb)
                            {
                                log.Info("update ME2N success");
                            }
                            else
                            {
                                log.Info("update ME2N failed");
                            }
                        }
                    }
                }
            }

            log.Info("============FO Completed ==========\n\n\n");
        }


        private static String SQL_UPDATE_T2_MATERIAL_FOR_FO_with_GR = "update t2_material set farmout_po_line=@po_ln, farmout_eta=@rm_eta, " +
            " farmout_gr_date=@rm_gr where id=@id";

        private static String SQL_UPDATE_T2_MATERIAL_FOR_FO_without_GR = "update t2_material set farmout_po_line=@po_ln, farmout_eta=@rm_eta " +
            "   where id=@id";

        private static bool doUpdatet2MaterialforFO(ME2NDao me2ndao, int id)
        {
            ArrayList al = new ArrayList();
            using (SqlConnection con = new SqlConnection(MYGlobal.getCString()))
            {
                con.Open();
                String sql = "";


                if (me2ndao.GRTransDate != null)
                {
                    sql = SQL_UPDATE_T2_MATERIAL_FOR_FO_with_GR;
                }
                else
                {
                    sql = SQL_UPDATE_T2_MATERIAL_FOR_FO_without_GR;
                }


                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    try
                    {
                        String po_item_ = me2ndao.PONum + "_" + me2ndao.POItemNum;
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@po_ln", po_item_);
                        cmd.Parameters.AddWithValue("@rm_eta", me2ndao.LastDelvDate);

                        if (me2ndao.GRTransDate != null)
                        {
                            cmd.Parameters.AddWithValue("@rm_gr", me2ndao.GRTransDate);
                        }


                        //where
                        cmd.Parameters.AddWithValue("@id", id);

                        log.Info("t2_material update sql=" + sql + ", material_id=" + id + ", po_item_ " + po_item_ + ", Last Del Date=" + me2ndao.LastDelvDate + ", GR-Transdate=" + me2ndao.GRTransDate);

                        int tt = cmd.ExecuteNonQuery();
                        log.Info("Update material result : " + tt);
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


        private static String SQL_ME2N_GET_PO_SQL = "";// "select  * from t_me2n where pr_num =@PR_NUM  and pr_item_num=@PR_ITEM_NUM;";
        private static ArrayList getAllMe2N4PR(String prnum, int pritemnum)
        {

            SQL_ME2N_GET_PO_SQL = "select  * from view_me2n_gr WITH (NOLOCK) " +
                " where pr_num like '%" + prnum + "%'  and pr_item_num=" + pritemnum;

            ArrayList al = new ArrayList();
            using (SqlConnection con = new SqlConnection(MYGlobal.getCString()))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(SQL_ME2N_GET_PO_SQL, con))
                {
                    cmd.CommandType = CommandType.Text;
                    // cmd.Parameters.AddWithValue("@PR_NUM", "00"+prnum);
                    //cmd.Parameters.AddWithValue("@PR_ITEM_NUM", pritemnum);

                   log.Info("  ME2N sql ="+ SQL_ME2N_GET_PO_SQL );

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            try
                            {
                                ME2NDao dao = new ME2NDao();
                                dao.PlantId = (string)reader["plant_id"];
                                // dao.Id = (Int32)reader["id"];

                                if (reader["po_num"] != DBNull.Value)
                                {
                                    //if no Po-num dont add it
                                    dao.PONum = (String)reader["po_num"];
                                    log.Info("Got PO Num : " + dao.PONum);
                                }
                                else
                                {
                                    continue;
                                }


                                if (reader["po_item"] != DBNull.Value)
                                {
                                    dao.POItemNum = (Int32)reader["po_item"];
                                }
                                else
                                {
                                    continue;
                                }


                                if (reader["material_no"] != DBNull.Value)
                                {
                                    dao.MaterialNum = (String)reader["material_no"];
                                }

                                dao.POQty = (double)reader["po_qty"];
                                dao.LastDelvDate = (string)reader["last_delivery_date"];
                                dao.NoOfDelv = (Int32)reader["no_of_deliveries"];
                                dao.LastUpdated = (DateTime)reader["last_updated"];


                                if (reader["movement_type"] != DBNull.Value)
                                {
                                    dao.GRMomentType = (Int32)reader["movement_type"];
                                }

                                if (reader["trans_date"] != DBNull.Value)
                                {
                                    dao.GRTransDate = (String)reader["trans_date"];
                                }

                                if (reader["vendor_num"] != DBNull.Value)
                                {
                                    dao.GRVendorNum = (String)reader["vendor_num"];
                                }

                                if (dao.PONum != null)
                                {
                                    log.Info("Got FO ME2N id= " + dao.Id + ", PONum= " + dao.PONum + ",POItem=" + dao.POItemNum + ", qty=" + dao.POQty);
                                    al.Add(dao);
                                }


                            }
                            catch (Exception ee)
                            {
                                log.Error("Error in FO getAllMe2N4PR : " + ee.Message);
                            }
                        }
                    }
                }
            }

            return al;
        }
         
        
        private static String SQL_T2_MATERIAL_IN_PROGRESS_RMTKPRQ = "select  * from t2_material WITH (NOLOCK)  " +
            " where material_status='In-Progress' and len(farmout_preq)>0 ";
        private static ArrayList getAllInProgressMaterial()
        {
            ArrayList al = new ArrayList();
            using (SqlConnection con = new SqlConnection(MYGlobal.getCString()))
            {
                con.Open();
                log.Info("FO SQL = "+ SQL_T2_MATERIAL_IN_PROGRESS_RMTKPRQ);
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

                            dao.Farmout_PReq = (String)reader["farmout_preq"];
                            dao.Farmout_PR_LN = (Int32)reader["farmout_pr_line"];

                            log.Info("Got material for FO " + dao.Id);
                            al.Add(dao);
                        }
                    }
                }
            }

            return al;
        }
    }
}
