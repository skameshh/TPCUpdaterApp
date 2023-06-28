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
 * Update vendor name and code from the Material master DB to t2_material
 * 
 * 1. Get all vendor null records from the t2_material
 * 2. get vendor no and name from Material Master
 * 3. Update t2_material 
 * 4. added in GlobalReportHelper.doGlobalReport()
 * 
 * 
 *  Sep 28th 
 *  Vendor updator and ProcurementDIQ  are same.. so rest this app
 */
namespace TPC2UpdaterApp.Helpers
{
    public class VendorUpdator
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public static void doFetchFromMMAndUpdateMaterial()
        {
            /*ArrayList al = getAllMaterialHasPurchaseOrder();
            for(int x = 0; x < al.Count; x++)
            {
                try
                {
                    T2MaterialDao dao = (T2MaterialDao)al[x];
                    String preq = dao.RMTKPUR_PReq;
                    int ln = dao.PR_LN_NBR;

                    *//*  String po_ln = dao.RMTKPUR_POLN;//4514793431_1

                    String po = po_ln.Substring(0, po_ln.IndexOf("_"));
                    String strlN = po_ln.Substring(po_ln.IndexOf("_"));
                    strlN = strlN.Replace("_", "");
                    int ln = Convert.ToInt32(strlN);*//*

                    VendorDao vdao = getVendorInfo(preq, ln);

                    updateMaterial(dao.Id, vdao);
                }catch(Exception ee)
                {
                    log.Info(ee.Message);
                }
            }*/
        }

        private static void updateMaterial(int matId, VendorDao vdao)
        {
            String sql = "update t2_material set vendor_name=@vendor_name, vendor_code=@vendor_code where id=@ID";

            using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
            {
                cnn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, cnn))
                {
                    try
                    {
                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.AddWithValue("@ID", matId);
                        cmd.Parameters.AddWithValue("@vendor_name", vdao.VendorName);
                        cmd.Parameters.AddWithValue("@vendor_code", vdao.PrchGroup);

                        //exe
                        int affRows = cmd.ExecuteNonQuery();
                        log.Info("Affrows = " + affRows);
                    }catch(Exception ee)
                    {
                        log.Error(ee.Message);
                    }
                }
            }
        }

        private static VendorDao getVendorInfo(String pr, int pr_lnumber)
        {
            VendorDao dao = new VendorDao();
            String sql = " select  pr.FIXED_VENDOR, v.Vendor_Name from PURCHASE_REQUISITIONS pr inner join VENDOR_NAMES v on pr.FIXED_VENDOR=v.Vendor_Num " +
                " where pr.PURCHASE_REQ_NUM='" + pr + "' and  pr.PURCHASE_REQ_ITEM="+ pr_lnumber;

            using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString4HCTNADBS()))
            {
                cnn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, cnn))
                {
                    cmd.CommandType = CommandType.Text;
                    log.Info("getVendorInfo()  sql = " + sql);

                    //cmd.Parameters.Add(new SqlParameter("@plant", plant));
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {                               

                                if ((reader["FIXED_VENDOR"]) != DBNull.Value)
                                {
                                    dao.VendorCode = ((string)reader["FIXED_VENDOR"]);
                                }

                                if ((reader["Vendor_Name"]) != DBNull.Value)
                                {
                                    dao.VendorName = ((string)reader["Vendor_Name"]);
                                }
                            }
                            catch (Exception ee)
                            {
                                log.Info(ee.Message);
                            }
                        }
                    }
                }
            }


            return dao;
        }


        private static VendorDao getVendorInfoOld(String purchaseOrder, int lnumber)
        {
            VendorDao dao = new VendorDao();
            String sql = "SELECT  po.PRCH_GRP_CD, po.PRCH_ORG_CD,  po.PO_NBR, po.PO_ITM_NBR, po.MTRL_NBR, po.VNDR_NBR, " +
                " vn.Vendor_Name   FROM [CPS_DATA].[dbo].[PURCHASE_ORDERS] po  WITH (NOLOCK) , VENDOR_NAMES vn  WITH (NOLOCK)   " +
                " where po.VNDR_NBR = vn.Vendor_Num " +
                " and po. po_nbr like '"+ purchaseOrder + "' and po.PO_ITM_NBR = "+ lnumber;

            using (SqlConnection cnn = new SqlConnection(DBUtils.getMaterialMasterDbCString()))
            {
                cnn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, cnn))
                {
                    cmd.CommandType = CommandType.Text;
                    log.Info("getVendorInfo()  sql = " + sql);

                    //cmd.Parameters.Add(new SqlParameter("@plant", plant));
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try { 
                                if ((reader["PRCH_GRP_CD"]) != DBNull.Value)
                                {
                                    dao.PrchGroup = ((string)reader["PRCH_GRP_CD"]);
                                }

                                if ((reader["PRCH_ORG_CD"]) != DBNull.Value)
                                {
                                    dao.PrchOrg = ((string)reader["PRCH_ORG_CD"]);
                                }

                                if ((reader["VNDR_NBR"]) != DBNull.Value)
                                {
                                    dao.VendorCode = ((string)reader["VNDR_NBR"]);
                                }

                                if ((reader["Vendor_Name"]) != DBNull.Value)
                                {
                                    dao.VendorName = ((string)reader["Vendor_Name"]);
                                }
                                }
                            catch (Exception ee)
                            {
                                log.Info(ee.Message);
                            }
                        }
                    }
                }
            }


                return dao;
        }


        public static ArrayList getAllMaterialHasPurchaseOrder()
        {
            ArrayList al = new ArrayList();
            try
            {
                //p.plant = @plant and
                String sql = " select  * from t2_material   WITH (NOLOCK) " +
                    " where len( RMl_TK_and_purch_part_po_ln)>1 and vendor_name is null and vendor_code is null";

                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.CommandType = CommandType.Text;
                        log.Info("getAllMaterialHasPurchaseOrder()  sql = " + sql);
                       
                        //cmd.Parameters.Add(new SqlParameter("@plant", plant));
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                try { 
                                    T2MaterialDao dao = new T2MaterialDao();

                                    dao.Id = (int)reader["id"];

                                    if ((reader["RMl_TK_and_purch_part_po_ln"]) != DBNull.Value)
                                    {
                                    dao.RMTKPUR_POLN = ((string)reader["RMl_TK_and_purch_part_po_ln"]);                                    
                                    }

                                    if(reader["RMl_TK_and_purch_part_Preq"]!= DBNull.Value)
                                    {
                                        dao.RMTKPUR_PReq = (String)reader["RMl_TK_and_purch_part_Preq"];
                                    }

                                    if (reader["PR_line"] != DBNull.Value)
                                    {
                                        dao.PR_LN_NBR = (int)reader["PR_line"];
                                    }


                                        //add it to ArrayList
                                        al.Add(dao);
                                }
                                catch (Exception ee)
                                {
                                    log.Info("getAllMaterialHasPurchaseOrder = " + ee.Message);
                                }
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


    }

   

}
