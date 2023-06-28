using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPC2UpdaterApp.DB;

namespace TPC2UpdaterApp.Helpers
{
    public class PurchasePriceUpdateHelper
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public static void doUpdatePurchasePartsPrice()
        {
            ArrayList matllist = getAllT2MaterialsForPurchaseOrders();
        }

        private static ArrayList getAllT2MaterialsForPurchaseOrders()
        {
            ArrayList al = new ArrayList();
            string sql = "select * from t2_material where len(RMl_TK_and_purch_part_po_ln)>1 and desc_1 is null";
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        log.Info("getAllT2MaterialsForPurchaseOrders() sql=" + sql);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                MBMaterialDao dao = new MBMaterialDao();
                                dao.MatlId = (Int32)reader["Id"];
                                dao.MaterialNum = (String)reader["material_num"];
                                dao.MaterialStatus = (String)reader["material_status"];

                                if ((reader["RMl_TK_and_purch_part_po_ln"]) != DBNull.Value)
                                {
                                    dao.RMTKPurchPartPOLine = (String)reader["RMl_TK_and_purch_part_po_ln"];
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
