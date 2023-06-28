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
* This using in GlobalReportHelper
* 
*/
namespace TPC2UpdaterApp.Helpers
{
   public class TPCMasterRouting
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /**
         * 1. get all TPC Material list
         * 2. Get Plant matereial status for each row
         * 3. update TPC master list table
         * 
         */
        public static void doMasterRouting()
        {
            ArrayList matllist = getAllT2Materials();
            if (matllist.Count == 0)
            {
                log.Info("No material found \n\n");
                return;
            }
            for (int x = 0; x < matllist.Count; x++)
            {
                MBMaterialDao dao = (MBMaterialDao)matllist[x];
                string pms = getPlantMatrlStatus(dao.MaterialNum);
                //
                doUpdateMaterialTable(dao.MatlId, pms);
            }
            log.Info("Master routing done\n\n");
        }


        private static void doUpdateMaterialTable(int id, string pms)
        {
            string sql = "update t2_material set sap_material_status ='" + pms + "' where id=" + id;
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.CommandType = CommandType.Text;
                        log.Info("doUpdateMaterialTable()  sql = " + sql);

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



        private static string getPlantMatrlStatus(String mn )
        {
            string pms = null;
            string sql = " SELECT  Plant_Matl_Status  FROM [MATERIAL_MASTER_CPS_PLANT] WITH (NOLOCK)  where Material like '%" + mn + "%' and plant = '2088'";
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString4HCTNADBS()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.CommandType = CommandType.Text;
                        log.Info("getPlantMatrlStatus() sql=" + sql);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {                                

                                try
                                {
                                   
                                    if ((reader["Plant_Matl_Status"]) != DBNull.Value)
                                    {
                                        pms = (string)reader["Plant_Matl_Status"];
                                    }
                                    
                                }
                                catch (Exception ee)
                                {
                                    log.Error("getPlantMatrlStatus Erro " + ee.Message);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                log.Error("Error in   getPlantMatrlStatus() " + ee.Message);
            }
            return pms;
        }


        //Sep 13th - Dont do Z4 here.. let it be done manually in the TPCPhase2App
        //Z4, Z2, ZF
        private static ArrayList getAllT2Materials()
        {
            ArrayList al = new ArrayList();
            string sql = "  select  id, material_num,material_status,total_req_qty   from t2_material WITH (NOLOCK) where routing_status in ('Mstr-Completed', 'Completed')   " +
                " and material_status = 'In-Progress' and sap_material_status not in ('Z4')  order by id desc; ";
            // and sap_material_status not in ('Z4') and sap_material_status is null  
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.CommandType = CommandType.Text;
                        log.Info("getAllT2Materials() sql=" + sql);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                MBMaterialDao dao = new MBMaterialDao();
                                dao.MatlId = (Int32)reader["Id"];
                                dao.MaterialNum = (String)reader["material_num"];
                                dao.MaterialStatus = (String)reader["material_status"];//[total_req_qty]
                                dao.TotReqQty = (Int32)reader["total_req_qty"];
/*
                                if ((reader["RMl_TK_and_purch_part_po_ln"]) != DBNull.Value)
                                {
                                    dao.RMTKPurchPartPOLine = (String)reader["RMl_TK_and_purch_part_po_ln"];
                                    log.Info("RMl_TK_and_purch_part_po_ln = " + dao.RMTKPurchPartPOLine);
                                }*/

                                al.Add(dao);
                            }
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                log.Error("Error in getting getAllT2Materials() " + ee.Message);
            }
            return al;
        }
    }
}
