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

namespace TPC2UpdaterApp.Helpers
{
    class TPCMakePartsriceUpdateHelper
    {

        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public static void doUpdateMakePartsPrice()
        {
            ArrayList matllist = getAllT2MaterialsForMakePartsOrders();
            for (int x = 0; x < matllist.Count; x++)
            {
                MBMaterialDao dao = (MBMaterialDao)matllist[x];

                string prod_zewo = dao.ProdZewo;

                decimal cost = getMakePartsPriceFor(prod_zewo);
                    Thread.Sleep(200);
                //update t2_material
                if (cost < 1)
                {
                    continue;
                }
                string str_cost = "USD " + cost;
                    doUpdateMaterialTable(dao.ProdZewo, str_cost);
                    Thread.Sleep(200); 
            }
        }


        private static void doUpdateMaterialTable(string  prodzewo, string cost )
        {
            string sql = "update t2_material set desc_1='" + cost + "' where prod_ZEWO='" + prodzewo + "'"  ;
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


        // >     select sum(COST_USD) as po107041057  FROM [CAR01_CPS_RPTS].[dbo].[GLPCA_PRODUCTION_ORDER_COST_DETAIL] where plant='2088' 
        //and order_number like '%107041057%' and COST_USD>0;
        private static decimal getMakePartsPriceFor(String prodzewo)
        {
            decimal cost = 0;
            string sql = "  select sum(COST_USD) as pocost  FROM  [GLPCA_PRODUCTION_ORDER_COST_DETAIL] WITH (NOLOCK) where  " +
                "    order_number like '%"+ prodzewo + "%' and COST_USD> 0; ";
            
            try
            {
                //DB connection account
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString4SCGREPORTING())) 
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
                                try
                                {
                                     
                                    if ((reader["pocost"]) != DBNull.Value)
                                    {
                                        cost = (decimal)reader["pocost"];
                                    }
                                    
                                }
                                catch (Exception ee)
                                {
                                    log.Error("Erro " + ee.Message);
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
            return cost;
        }

        private static ArrayList getAllT2MaterialsForMakePartsOrders()
        {
            ArrayList al = new ArrayList();
            string sql = "select  Id, material_num, material_status, total_req_qty, prod_ZEWO," +
                " * from t2_material WITH (NOLOCK) where material_status='In-Progress' and len(prod_ZEWO)>0  and " +
                " routing_scope in ('In-House', 'Farm-Out', 'Turn Key', 'Service')  order by id desc; "; // and desc_1 is null
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.CommandType = CommandType.Text;
                        log.Info("getAllT2MaterialsForMakePartsOrders() sql=" + sql);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                MBMaterialDao dao = new MBMaterialDao();
                                dao.MatlId = (Int32)reader["Id"];
                                dao.MaterialNum = (String)reader["material_num"];
                                dao.MaterialStatus = (String)reader["material_status"];//[total_req_qty]
                                dao.TotReqQty = (Int32)reader["total_req_qty"];
                                dao.ProdZewo = (String)reader["prod_ZEWO"]; 

                                
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
