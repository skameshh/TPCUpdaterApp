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
using TPC2UpdaterApp.Helpers;
using TPC2UpdaterApp.ProcurementDIQ;
using TPC2UpdaterApp.warehouse;

/**
 * 
 * GlobalReportJob
 * 
 */
namespace TPC2UpdaterApp.Forms
{
   public class GlobalReportHelper
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /**
        * Make Parts
        * 1. Fetch all In-Progress material for (Assembly, Formout, In-House, turnkey)
        * 2. connect to 	Ø AP1REPORTING , DB:AP101_MPM , with Production order , fetch 
        * select  PROD_ORDR_NBR, TOT_ORDR_QTY, ACT_CMPLT_DT ,  PROD_ORDR_DSC
           from PROD_ORDR where PROD_ORDR_NBR like '%106771172%';
        * 
        * Buy Parts
        * 1.Manual PO ( Rtg Scope: purchase + RM Assign: (null) = Manual)
        * just follow RMTK_GR date 
        * 
        * 2. System PO (Rtg Scope: purchase + RM Assign: Tagged = system generated )
        *  Connect to HCTNADBS001 -> CPS_DATA
        * select h.Material_Doc, h.Transaction_Date_Only, h.Material_NOZERO, h.Wip_Order, h.Wip_Order_Type 
               from MATERIAL_ISSUE_HISTORY h  where h.Wip_order like '%106773350%' ;
        * 
        */
        public static void doGlobalReport()
        {
            try { 
               doMakeParts();

               doBuyPartsSystemPO();//BUY_PARTS_SPO

               doBuyPartsManualPO();//BUY_PARTS_SPO 

                //SP_T2_GLOBAL_REPORT
                doUpdateGlobalReport();

                //exec SP_GEN_GLOBAL_REPORT '2022-10-30'
                doUpdateGlobalReport2();

            }
            catch (Exception ee)
            {
                log.Info("EE = " + ee.Message);
            }

            try
            {
                //Update vendor name
                Thread.Sleep(5000);
                VendorUpdator.doFetchFromMMAndUpdateMaterial();
            }
            catch (Exception ee)
            {
                log.Info("EE = " + ee.Message);
            }


            try
            {
                Thread.Sleep(5000);
                TechInventoryDBUpdator.doTechInventory();
            }
            catch(Exception ee)
            {
                log.Info("EE = " + ee.Message);

            }


            try
            {
                Thread.Sleep(5000);
                TPCPPPriceUpdateHelper.doUpdatePurchasePartsPrice();               
            }
            catch (Exception ee)
            {
                log.Info("EE = " + ee.Message);

            }


            try
            {
                Thread.Sleep(5000);               
                TPCMakePartsriceUpdateHelper.doUpdateMakePartsPrice();
            }
            catch (Exception ee)
            {
                log.Info("EE = " + ee.Message);
            }


            try
            {
                Thread.Sleep(5000);
                PlannerAppoxiEstmateDate.doUpdateApproxPromixedDate();

            }
            catch (Exception ee)
            {
                log.Info("EE = " + ee.Message);
            }


            try
            {
                Thread.Sleep(5000);
                TPCMasterRouting.doMasterRouting();

            }
            catch (Exception ee)
            {
                log.Info("EE = " + ee.Message);
            }


            try
            {
                Thread.Sleep(5000);
                //working on it
                TurnkeyFarmoutHelper.doUpdateFarmoutTurnkey();
            }
            catch (Exception ee)
            {
                log.Error("Error doQCUpdate() " + ee.Message);
            }

            try
            {
                Thread.Sleep(5000);
                ProcurementPOPRUpdate.doUpdate();

                ProcurementTKFOPRUpdate.doUpdate();
            }
            catch (Exception ee)
            {
                log.Error("Error doQCUpdate() " + ee.Message);
            }

             

        }


        private static String MAKE_PARTS = "Make";
        private static String BUY_PARTS_SPO = "BuySPO";//system po
        private static String BUY_PARTS_MPO = "BuyMPO";//manual po


        public static void doUpdateGlobalReport()
        {
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand("SP_T2_GLOBAL_REPORT", cnn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        log.Info("doUpdateGlobalReport() query : SP_T2_GLOBAL_REPORT , ");
                        cmd.Parameters.Add(new SqlParameter("@IN_DATE", DateTime.Now.ToString("yyyy-MM-dd")));
                       

                        int affRows = cmd.ExecuteNonQuery();
                        log.Info("doUpdateGlobalReport Affrows = " + affRows);
                    }
                }
            }
            catch (Exception ee)
            {

                log.Error("Error in doUpdateGlobalReport() = " + ee.Message + "\n\n");
            }
        }

        public static void doUpdateGlobalReport2()
        {
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand("SP_GEN_GLOBAL_REPORT", cnn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        log.Info("doUpdateGlobalReport() query : SP_GEN_GLOBAL_REPORT , ");
                        cmd.Parameters.Add(new SqlParameter("@IN_DATE", DateTime.Now.ToString("yyyy-MM-dd")));


                        int affRows = cmd.ExecuteNonQuery();
                        log.Info("doUpdateGlobalReport2 Affrows = " + affRows);
                    }
                }
            }
            catch (Exception ee)
            {

                log.Error("Error in doUpdateGlobalReport2() = " + ee.Message + "\n\n");
            }
        }




        private static void doUpdateMaterialTable(DateTime act_compl_date, int id, String make_or_buy)
        {
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand("SP_UPDATE_T2_MATERIAL_FOR_GLOBAL_REPORT", cnn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        log.Info("doUpdateMaterialTable() query : SP_UPDATE_T2_MATERIAL_FOR_GLOBAL_REPORT , for id=" + id + ", date=" + act_compl_date + ", make_or_buy " + make_or_buy);
                        cmd.Parameters.Add(new SqlParameter("@ACT_COMPLD_DATE", act_compl_date.ToString("yyyy-MM-dd")));
                        cmd.Parameters.Add(new SqlParameter("@ID", id));
                        cmd.Parameters.Add(new SqlParameter("@MAKEORBUY", make_or_buy));

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

        /**
         * Server HCTNADBS001 , 
         * select h.Material_Doc, h.Transaction_Date_Only, h.Material_NOZERO, h.Wip_Order, h.Wip_Order_Type 
 from MATERIAL_ISSUE_HISTORY h  where h.Wip_order like '%106773350%' ;
         (sys po = 2830, 2652)
         */
        private static DateTime getAllBuyPartsSystePOComplDate(String prodOrdNbr, String matlnbr, int row)
        {
            DateTime act_compl_date = default(DateTime);
            String matrl_doc = "";
            String matl_nbr = "";
            String wip_order = "";
            String sql = "select h.Material_Doc, h.Transaction_Date_Only, h.Material_NOZERO, h.Wip_Order, h.Wip_Order_Type " +
                " from MATERIAL_ISSUE_HISTORY h   WITH (NOLOCK)  where h.Wip_order like \'%" + prodOrdNbr + "%\' and Material_NOZERO='" + matlnbr + "'";

            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString4HCTNADBS()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.CommandType = CommandType.Text;
                        log.Info("getAllBuyPartsSystePOComplDate() query :" + sql);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                try
                                {
                                    if ((reader["Material_Doc"]) != DBNull.Value)
                                    {
                                        matrl_doc = (String)reader["Material_Doc"];
                                    }
                                    if ((reader["Transaction_Date_Only"]) != DBNull.Value)
                                    {
                                        act_compl_date = (DateTime)reader["Transaction_Date_Only"];
                                    }
                                    if ((reader["Material_NOZERO"]) != DBNull.Value)
                                    {
                                        matl_nbr = (String)reader["Material_NOZERO"];
                                    }
                                    if ((reader["Wip_Order"]) != DBNull.Value)
                                    {
                                        wip_order = (String)reader["Wip_Order"];
                                    }

                                    log.Info("prodOrdNbr=" + prodOrdNbr + ", row=" + row + ", matrl_doc=" + matrl_doc + ", act_compl_date=" + act_compl_date + ", matl_nbr=" + matl_nbr
                                        + ", wip_order=  " + wip_order);
                                }
                                catch (Exception ee)
                                {

                                    log.Error("error in getAllBuyPartsSystePOComplDate() " + ee.Message);
                                }
                            }
                        }
                    }
                }
            }catch(Exception ee)
            {
                log.Error("error in getAllBuyPartsSystePOComplDate() main=> " + ee.Message);
            }

            return act_compl_date;
        }


        /**
         * 'Assembly', 'Farm-Out', 'In-House', 'Turn Key'
         * 
         */
        private static void doMakeParts()
        {
            ArrayList list_make_parts = getAllTPCMakeBuyParts(FETCH_MAKE_PARTS);
            log.Info("list_make_parts = " + list_make_parts.Count);

            for (int x = 0; x < list_make_parts.Count; x++)
            {

                MBMaterialDao dao = (MBMaterialDao)list_make_parts[x];
                if (dao.ProdZewo.Equals(DBNull.Value))
                {
                    continue;
                }
                if (dao.ProdZewo.Length < 1)
                {
                    continue;
                }
                //getCString4AP1REPORTING
                DateTime act_compl_date = getAllMakePartsActCompletionDate(dao.ProdZewo, x);
                if (act_compl_date != default(DateTime))
                {
                    doUpdateMaterialTable(act_compl_date, dao.MatlId, MAKE_PARTS);
                }

                //update
            }
            log.Info("=============== MAKE PARTS COMPLETED ============\n\n");
        }

        /**
         * 
         * 
         */
        private static void doBuyPartsSystemPO()
        {
            // System PO == 
            ArrayList list_buy_parts_sys_po = getAllTPCMakeBuyParts(FETCH_BUY_PARTS_SYSTEM_PO);
            log.Info("list_buy_parts_sys_po = " + list_buy_parts_sys_po.Count);

            for (int x = 0; x < list_buy_parts_sys_po.Count; x++)
            {
                MBMaterialDao dao = (MBMaterialDao)list_buy_parts_sys_po[x];
                if (dao.ProdZewo.Equals(DBNull.Value))
                {
                    continue;
                }
                if (dao.ProdZewo.Length < 1)
                {
                    continue;
                }
                DateTime act_compl_date = getAllBuyPartsSystePOComplDate(dao.ProdZewo, dao.MaterialNum, x);
                if (act_compl_date != default(DateTime))
                {
                   // save it in new column GI Date and put it in act comep date
                    doUpdateMaterialTable(act_compl_date, dao.MatlId, BUY_PARTS_SPO);
                }
            }

            log.Info("==============XXXX= BUY PARTS COMPLETED system po=XXXX===========\n\n");
        }

        private static void doBuyPartsManualPO()
        {
            ArrayList list_buy_parts_manual_po = getAllTPCMakeBuyParts(FETCH_MAKE_PARTS_MANUAL_PO);
            log.Info("list_buy_parts_manual_po = " + list_buy_parts_manual_po.Count);


            for (int x = 0; x < list_buy_parts_manual_po.Count; x++)
            {
                MBMaterialDao dao = (MBMaterialDao)list_buy_parts_manual_po[x];
                /*if (dao.ProdZewo.Equals(DBNull.Value))
                {
                    continue;
                }
                if (dao.ProdZewo.Length < 1)
                {
                    continue;
                }*/

                DateTime act_compl_date = dao.RMTKPurchGRDate;
                log.Info("RMTKPurchGRDate = " + act_compl_date);
                if (act_compl_date != default(DateTime) || (act_compl_date != null))
                {
                    if (act_compl_date.Year >= 2000)
                    {
                        doUpdateMaterialTable(act_compl_date, dao.MatlId, BUY_PARTS_MPO);
                    }

                }
            }


            log.Info("==============XXXX= BUY PARTS COMPLETED manual po=XXXX===========\n\n");
        }



        private static DateTime getAllMakePartsActCompletionDate(String prodOrdNbr, int row)
        {
            DateTime act_compl_date = default(DateTime);

            try
            {
                String sql = "select  PROD_ORDR_NBR, TOT_ORDR_QTY, ACT_CMPLT_DT ,  PROD_ORDR_DSC " +
            " from PROD_ORDR  WITH (NOLOCK)  where PROD_ORDR_NBR like \'%" + prodOrdNbr + "%\'";


                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString4AP1REPORTING()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.CommandType = CommandType.Text;
                        log.Info("getAllMakePartsActCompletionDate() query :" + sql);
                        // cmd.Parameters.Add(new SqlParameter("@prod_no", prodOrdNbr));

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                try
                                {

                                    String prod_ord = (String)reader["PROD_ORDR_NBR"];
                                    decimal qty = 0;

                                    String ord_desc = "";


                                    if ((reader["TOT_ORDR_QTY"]) != DBNull.Value)
                                    {
                                        qty = (decimal)reader["TOT_ORDR_QTY"];
                                    }

                                    if ((reader["ACT_CMPLT_DT"]) != DBNull.Value)
                                    {
                                        act_compl_date = (DateTime)reader["ACT_CMPLT_DT"];
                                    }

                                    if ((reader["PROD_ORDR_DSC"]) != DBNull.Value)
                                    {
                                        ord_desc = (String)reader["PROD_ORDR_DSC"];
                                    }



                                    log.Info(" current row =" + row + " , sql prodOrdNbr=" + prodOrdNbr + ",  returned prod_ord = " + prod_ord + " qty=" + qty + ", act_compl_date = " + act_compl_date + ", ord_desc=" + ord_desc);


                                }
                                catch (Exception ee)
                                {

                                    log.Error("Error getAllMakePartsActCompletionDate() : " + ee.Message);
                                }

                            }
                            log.Info("~ DON E!");
                        }
                    }

                }
            }
            catch (Exception ee)
            {

                log.Error("Error in getAllMakePartsActCompletionDate() " + ee.Message);
            }

            return act_compl_date;
        }



        private static int FETCH_MAKE_PARTS = 1;
        private static int FETCH_BUY_PARTS_SYSTEM_PO = 2;
        private static int FETCH_MAKE_PARTS_MANUAL_PO = 3;

        /**
         * 'Assembly', 'Farm-Out', 'In-House', 'Turn Key'
         * 
         */
        private static ArrayList getAllTPCMakeBuyParts(int action)
        {
            ArrayList al = new ArrayList();
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand("SP_GLOBAL_REPORT_MASTERLIST_SELECT", cnn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        log.Info("getAllMakeParts() query : SP_GLOBAL_REPORT_MASTERLIST_SELECT");
                        cmd.Parameters.Add(new SqlParameter("@ACTION", action));

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                MBMaterialDao dao = new MBMaterialDao();
                                dao.MatlId = (Int32)reader["matlId"];
                                dao.MaterialNum = (String)reader["material_num"];
                                dao.MaterialStatus = (String)reader["material_status"];
                                dao.RoutingScope = (String)reader["routing_scope"];
                                if ((reader["prod_ZEWO"]) != DBNull.Value)
                                {
                                    dao.ProdZewo = (String)reader["prod_ZEWO"];
                                }
                                if ((reader["raw_matl_assignment"]) != DBNull.Value)
                                {
                                    dao.RawMaterialAssignmet = (String)reader["raw_matl_assignment"];
                                }

                                if ((reader["RM_TK_Purch_GR"]) != DBNull.Value)
                                {
                                    dao.RMTKPurchGRDate = (DateTime)reader["RM_TK_Purch_GR"];
                                }

                                if ((reader["act_compltd_date_from_SAP"]) != DBNull.Value)
                                {
                                    dao.ActCompltdDateFromSAP = (DateTime)reader["act_compltd_date_from_SAP"];
                                }

                                al.Add(dao);
                            }
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                log.Error("Error in getting getAllMakeParts() " + ee.Message);
            }
            return al;
        }



    }//class
}
