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
    class TPCPPPriceUpdateHelper
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        
        public static void doUpdatePurchasePartsPrice()
        {
            ArrayList matllist = getAllT2MaterialsForPurchaseOrders();
            for(int x = 0; x < matllist.Count; x++)
            {
                MBMaterialDao dao = (MBMaterialDao)matllist[x];
                string po_line = dao.RMTKPurchPartPOLine;
                String po = po_line.Substring(0, po_line.IndexOf("_"));
                String str_line = po_line.Substring(po_line.IndexOf("_")+1);
                int qty = dao.TotReqQty;

                int line_num = Convert.ToInt32(str_line);

                log.Info("po_line = " + po_line+", po = " + po + ", line " + str_line);

                SAPPurchaseOrder pdao = getPurchasePriceForPurchaseOrders(po, line_num);
                String upd = string.Empty;
                if (pdao != null)
                {
                    log.Info(" local unit price  = " + pdao.Local_net_unit_price + ", currency = " + pdao.Local_currency
                        + ", PO_price_unit " + pdao.PO_price_unit);
                   /* try
                    {
                        upd = pdao.Local_currency + (qty * pdao.Local_net_unit_price);
                    }catch(Exception ee)
                    {
                        log.Info("error " + ee.Message);
                        upd = "err : "+ pdao.Local_net_unit_price;
                    }*/


                    try
                    {
                        upd = pdao.PO_Currency +" " + (qty * pdao.PO_Net_unit_price);
                    }
                    catch (Exception ee)
                    {
                        log.Info("error " + ee.Message);
                        upd = "err : " + pdao.Local_net_unit_price;
                    }



                    Thread.Sleep(200);
                    //update t2_material
                    doUpdateMaterialTable(dao.MatlId, upd);
                    Thread.Sleep(200);
                }
                

            }
        }


        private static void doUpdateMaterialTable(int id, string po_price )
        {
            string sql = "update t2_material set desc_1='"+ po_price+"' where id="+id;
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.CommandType = CommandType.Text;
                        log.Info("doUpdateMaterialTable()  sql = "+sql);                        

                        int affRows = cmd.ExecuteNonQuery();
                        log.Info("Affrows = " + affRows);
                    }
                }
            }
            catch (Exception ee)
            {

                log.Error("Error in doUpdateMaterialTable() = " + ee.Message + "\n\n");
            }
        }



        private static SAPPurchaseOrder getPurchasePriceForPurchaseOrders(String purchaseOrder, int line)
        {
            SAPPurchaseOrder dao = null;
            string sql = " select *  FROM [CPS_DATA].[dbo].[PURCHASE_ORDERS] WITH (NOLOCK) where PO_NBR='" + purchaseOrder+ "' and  PO_ITM_NBR="+ line;
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString4HCTNADBS()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.CommandType = CommandType.Text;
                        log.Info("getPurchasePriceForPurchaseOrders() sql=" + sql);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                  dao = new SAPPurchaseOrder();

                                try
                                {
                                    dao.PO_Currency = (string)reader["PO_CRNCY_CD"]; //use this
                                    dao.PO_Item_no = (int)reader["PO_ITM_NBR"];
                                    dao.PO_NBR = (string)reader["po_nbr"];
                                    dao.PO_Net_unit_price = (decimal)reader["PO_NET_UNT_PRC"]; //use this.
                                    dao.PO_price_unit = (decimal)reader["PRC_UNIT"];
                                    dao.VendorNbr = (string)reader["VNDR_NBR"];

 

                                    if ((reader["LOCAL_CRNCY_CD"]) != DBNull.Value)
                                    {
                                        dao.Local_currency = (string)reader["LOCAL_CRNCY_CD"];
                                    }
                                    if ((reader["LOCAL_NET_UNT_PRC"]) != DBNull.Value)
                                    {
                                        dao.Local_net_unit_price = (decimal)reader["LOCAL_NET_UNT_PRC"];
                                    }
                                }catch(Exception ee)
                                {
                                    log.Error("Erro "+ee.Message);
                                }                                
                            }
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                log.Error("Error in   getPurchasePriceForPurchaseOrders() " + ee.Message);
            }
            return dao;
        }

        private static ArrayList getAllT2MaterialsForPurchaseOrders()
        {
            ArrayList al = new ArrayList();
            string sql = "select Id,material_num, material_status, total_req_qty, RMl_TK_and_purch_part_po_ln, " +
                " * from t2_material WITH (NOLOCK) where len(RMl_TK_and_purch_part_po_ln)>1 " +
                "and  material_status='In-Progress' and len(ZNDPO)=0"; //desc_1 is  null and
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.CommandType = CommandType.Text;
                        log.Info("getAllT2MaterialsForPurchaseOrders() sql=" + sql);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                MBMaterialDao dao = new MBMaterialDao();
                                dao.MatlId = (Int32)reader["Id"];
                                dao.MaterialNum = (String)reader["material_num"];
                                dao.MaterialStatus = (String)reader["material_status"];//[total_req_qty]
                                dao.TotReqQty = (Int32)reader["total_req_qty"];

                                if ((reader["RMl_TK_and_purch_part_po_ln"]) != DBNull.Value)
                                {
                                    dao.RMTKPurchPartPOLine = (String)reader["RMl_TK_and_purch_part_po_ln"];
                                    log.Info("RMl_TK_and_purch_part_po_ln = "+dao.RMTKPurchPartPOLine);
                                }

                                al.Add(dao);
                            }
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                log.Error("Error in getting getAllT2MaterialsForPurchaseOrders() " + ee.Message);
            }
            return al;
        }

    }




}
