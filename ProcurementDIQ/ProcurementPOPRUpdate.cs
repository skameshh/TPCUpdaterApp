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
 *  put it in Globalhelper
 * just update pr date for in-progress items
 */
namespace TPC2UpdaterApp.ProcurementDIQ
{
   public class ProcurementPOPRUpdate
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

                    ME2NDao me2ndao = getAll(dao.RMTKPUR_PReq, dao.PR_LN_NBR);
                    if (me2ndao != null)
                    {
                        doUpdatet2MaterialforRmTK(me2ndao, dao.Id);
                    }
                   
                }
                log.Info("Update Done \n\n");
            }
            else
            {
                log.Info("No rows found \n\n");
            }
        }

      
        private static bool doUpdatet2MaterialforRmTK(ME2NDao me2ndao, int id)
        {
            string SQL_UPDATE_T2_MATERIAL = string.Empty;

            if (me2ndao.PONum == null)
            {
                SQL_UPDATE_T2_MATERIAL = "update t2_material set vendor_name='" + me2ndao.VendorName + "' , vendor_code='" + me2ndao.VendorCode + "'" +
                      " ,  PR_raised_on='" + me2ndao.PRCreatedDate.ToString("yyyy-MM-dd") + "', PR_raised_by='" + me2ndao.PRCreatedBy + "' " +
              "   where id=" + id;
            }
            else
            {
                string po_ln = me2ndao.PONum+"_"+ me2ndao.POItemNum;

                SQL_UPDATE_T2_MATERIAL = "update t2_material set RMl_TK_and_purch_part_po_ln='"+ po_ln + "'  , vendor_name='" + me2ndao .VendorName+ "' , vendor_code='"+ me2ndao .VendorCode+ "'" +
                    " ,  PR_raised_on='" + me2ndao.PRCreatedDate.ToString("yyyy-MM-dd") + "', PR_raised_by='" + me2ndao.PRCreatedBy + "' " +
            "   where id=" + id;
            }


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

                        log.Info("t2_material update sql=" + SQL_UPDATE_T2_MATERIAL  );

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


        private static ME2NDao  getAll(String reqNum, int item)
        {
            String SQL = " select v.Vendor_Name,  pr.FIXED_VENDOR, pr.PURCHASE_REQ_NUM, pr.PURCHASE_REQ_ITEM, pr.PLANT, pr.REQ_CREATE_DATE, pr.CREATED_BY, pr.MATERIAL, pr.PO_NUMBER, " +
            " pr.PO_ITEM  from PURCHASE_REQUISITIONS pr inner join VENDOR_NAMES v on pr.FIXED_VENDOR=v.Vendor_Num  " +
            " where PURCHASE_REQ_NUM like '%" + reqNum + "%' and PURCHASE_REQ_ITEM="+ item;
            log.Info("Req sql = "+ SQL);
            ME2NDao dao = null;
            using (SqlConnection con = new SqlConnection(MYGlobal.getHCTDBCString()))
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
                                if ((reader["PO_NUMBER"]) != DBNull.Value)
                                {
                                    dao.PONum = (String)reader["PO_NUMBER"];
                                }
                                else
                                {
                                    dao.PONum = null;
                                }

                                if ((reader["PO_ITEM"]) != DBNull.Value)
                                {
                                    dao.POItemNum = (int)reader["PO_ITEM"];
                                }

                                if ((reader["FIXED_VENDOR"]) != DBNull.Value)
                                {
                                    dao.VendorCode = (string)reader["FIXED_VENDOR"];
                                    dao.VendorName = (string)reader["Vendor_Name"];
                                }
                                else
                                {
                                    dao.VendorCode = null;
                                    dao.VendorName =null;
                                }
                                   
                                ;
                            }catch(Exception ee)
                            {
                                log.Error("Ee " + ee.Message);
                            }
                        }
                    }
                }
            }
            return dao;
        }

        private static String SQL_T2_MATERIAL_IN_PROGRESS_RMTKPRQ = "select * from t2_material  WITH (NOLOCK) " +
           " where material_status='In-Progress' and routing_scope='Purchase' and len(RMl_TK_and_purch_part_Preq)>0  and  len(RMl_TK_and_purch_part_po_ln)=0 ";
            // remove
        private static ArrayList getAllInProgressMaterial()
        {
            ArrayList al = new ArrayList();
            using (SqlConnection con = new SqlConnection(MYGlobal.getCString()))
            {
                con.Open();
                log.Info("Req sql = " + SQL_T2_MATERIAL_IN_PROGRESS_RMTKPRQ);

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

                            dao.RMTKPUR_PReq = (String)reader["RMl_TK_and_purch_part_Preq"];
                            dao.PR_LN_NBR = (Int32)reader["PR_line"];

                            log.Info("Got material " + dao.Id + ", PReq= " + dao.RMTKPUR_PReq + ", Preq LN=" + dao.PR_LN_NBR);
                            al.Add(dao);
                        }
                    }
                }
            }

            return al;
        }

    }
}
