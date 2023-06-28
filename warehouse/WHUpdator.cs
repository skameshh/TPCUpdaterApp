using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 *  Get all QC Done and WH Received for today
 *  7Am for yesterday records
 *  4pm for Today records
 * 
 */
namespace TPC2UpdaterApp.warehouse
{
    class WHUpdator
    {

        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static ArrayList doGetSOTKFOList(int action)
        {
            string sql = "";

            ArrayList al = new ArrayList();
            using (SqlConnection sqlCon = new SqlConnection(MYGlobal.getCString()))
            {
                sqlCon.Open();
                log.Info(" doGetSOTKFOList()  sql = SP_PROCUREMENT_DIQ_REPORT , Action=" + action);
                using (SqlCommand cmd = new SqlCommand(sql, sqlCon))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ACTION", action);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                string resource = string.Empty;
                                string mtrlNum = string.Empty;
                                string prodOrdNum = string.Empty;
                                string remarks_procurement = string.Empty;

                                int diq = 0;
                               ;

                                

                                if ((reader["material_num"]) != DBNull.Value)
                                {
                                    mtrlNum = ((string)reader["material_num"]);
                                    dao.MaterialNum = mtrlNum;
                                }

                                if ((reader["prod_ZEWO"]) != DBNull.Value)
                                {
                                    prodOrdNum = ((string)reader["prod_ZEWO"]);
                                    dao.ProdOrdNum = prodOrdNum;
                                }


                                if ((reader["remarks_procurement"]) != DBNull.Value)
                                {
                                    remarks_procurement = ((string)reader["remarks_procurement"]);
                                    dao.Remarks = remarks_procurement;
                                }

                                if ((reader["diq"]) != DBNull.Value)
                                {
                                    diq = ((int)reader["diq"]);
                                    dao.Diq = diq;
                                }

                                al.Add(dao);

                                log.Info("PP resource=" + resource + ", remarks_procurement=" + remarks_procurement + ", diq = " + diq);
                            }
                            catch (Exception ee)
                            {
                                log.Error("Error = " + ee.Message);
                            }
                        }
                    }

                }
            }

            return al;
        }

    }
}
