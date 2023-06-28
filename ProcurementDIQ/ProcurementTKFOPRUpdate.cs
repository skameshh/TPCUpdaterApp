using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPC2UpdaterApp.DB;

namespace TPC2UpdaterApp.ProcurementDIQ
{
   public class ProcurementTKFOPRUpdate
    {

        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void doUpdate()
        {
            ArrayList al = getAllInProgressMaterial();
            if (al.Count > 0)
            {
                for (int x = 0; x < al.Count; x++)
                {
                    T2MaterialDao dao = (T2MaterialDao)al[x];

                    ME2NDao me2ndao = getAll(dao.ProdZEWO);
                    if (me2ndao != null)
                    {
                        doUpdatet2MaterialforRmTK(me2ndao, dao.Id);
                    }

                }
            }
            else
            {
                log.Info("No rows found \n\n");
            }
        }

        private static bool doUpdatet2MaterialforRmTK(ME2NDao me2ndao, int id)
        {
            string SQL_UPDATE_T2_MATERIAL = "update t2_material set PR_raised_on='"+ me2ndao.PRCreatedDate.ToString("yyyy-MM-dd") + "',PR_raised_by='"+ me2ndao.PRCreatedBy + "' , RMl_TK_and_purch_part_Preq='" + me2ndao.PRNum+ "', PR_line="+ me2ndao.PRItemNu +" where id="+id;

            ArrayList al = new ArrayList();
            using (SqlConnection con = new SqlConnection(MYGlobal.getCString()))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(SQL_UPDATE_T2_MATERIAL, con))
                {
                    try
                    {
                        //String po_item_ = me2ndao.PONum + "_" + me2ndao.POItemNum;
                        cmd.CommandType = CommandType.Text;
                        //where
                        //cmd.Parameters.AddWithValue("@id", id);

                        log.Info("t2_material update sql=" + SQL_UPDATE_T2_MATERIAL);

                        int tt = cmd.ExecuteNonQuery();
                        log.Info("doUpdatet2MaterialforRmTK resule : " + tt);
                        return true;
                    }
                    catch (Exception ee)
                    {
                        log.Info("Update Me2n failed Error : " + ee.Message);
                        return false;
                    }
                }
            }
        }

        private static ME2NDao getAll(String zewo)
        {
            String SQL = "  SELECT TOP (1000) PURCHASE_REQ_NUM, PURCHASE_REQ_ITEM, REQ_CREATE_DATE, CREATED_BY, MATERIAL, PURCHASE_REQ_QTY, PROD_ORDR_NBR, PROD_ORDR_OPR , " +
                "PO_NUMBER, PO_ITEM   " +
                "FROM[CPS_DATA].[dbo].[PURCHASE_REQUISITIONS] where PROD_ORDR_NBR like '%"+ zewo + "%'; "; //107336278


            log.Info("Req sql = " + SQL);
            ME2NDao dao = null;
            using (SqlConnection con = new SqlConnection(MYGlobal.getHCTDBCString()))//CS_DATA
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(SQL, con))
                {
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                dao = new ME2NDao();
                                dao.PRCreatedDate = (DateTime)reader["REQ_CREATE_DATE"];
                                dao.PRCreatedBy = (String)reader["CREATED_BY"];
                                if ((reader["PURCHASE_REQ_NUM"]) != DBNull.Value)
                                {
                                    dao.PRNum = (String)reader["PURCHASE_REQ_NUM"];

                                    if (dao.PRNum.Length > 0)
                                    {
                                        string ss = dao.PRNum.Substring(0,2);
                                        if (ss.Equals("00"))
                                        {
                                            dao.PRNum = dao.PRNum.Substring(2);                                            
                                        }
                                        log.Info("Prnum " + dao.PRNum);
                                    }
                                }
                                else
                                {
                                    dao.PRNum = null;
                                }

                                if ((reader["PURCHASE_REQ_ITEM"]) != DBNull.Value)
                                {
                                    dao.PRItemNu = (int)reader["PURCHASE_REQ_ITEM"];
                                }

                            }
                            catch (Exception ee)
                            {
                                log.Error("Ee " + ee.Message);
                            }
                        }
                    }
                }
            }
            return dao;
        }

        //and len(RMl_TK_and_purch_part_Preq)=0
        private static String SQL_T2_MATERIAL_IN_PROGRESS_RMTKPRQ ="select   material_num, material_desc, total_req_qty, prod_ZEWO, routing_scope,  RMl_TK_and_purch_part_po_ln ,RMl_TK_and_purch_part_Preq,id, " +
            "project_id from t2_material  where routing_scope in ('Farm-Out','Turn Key') and RMl_TK_and_purch_part_po_ln is null and len(prod_zewo)>0  " +
            "and material_status = 'In-Progress' order by id desc "; // len(RMl_TK_and_purch_part_po_ln)=0 ==> add this 

        private static ArrayList getAllInProgressMaterial()
        {
            ArrayList al = new ArrayList();
            using (SqlConnection con = new SqlConnection(MYGlobal.getCString()))
            {
                log.Info(" getAllInProgressMaterial() SQL_T2_MATERIAL_IN_PROGRESS_RMTKPRQ = " + SQL_T2_MATERIAL_IN_PROGRESS_RMTKPRQ);
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
                            dao.ProdZEWO = (string)reader["prod_ZEWO"];
                           //dao.RMTKPUR_PReq = (String)reader["RMl_TK_and_purch_part_Preq"];
                           // dao.PR_LN_NBR = (Int32)reader["PR_line"];

                            log.Info("Got material " + dao.Id + ", prod Zewo = " + dao.ProdZEWO );
                            al.Add(dao);
                        }
                    }
                }
            }

            return al;
        }
    }
}
