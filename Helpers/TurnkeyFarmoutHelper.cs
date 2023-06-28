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
 * 
 * Triggering in QCHelper QCHelper also in GlobalReportHelper
 */
namespace TPC2UpdaterApp.Forms
{
   public class TurnkeyFarmoutHelper
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
              
        public static void doUpdateFarmoutTurnkey()
        { 
            ArrayList al = getAllTPCMaterials();//OK
            log.Info("Total Materials "+ al.Count);
            if (al != null)
            {
                for (int x = 0; x < al.Count; x++)//al.Count
                {
                    String prozewo = (String)al[x];
                    ArrayList alSch = getAllOperationsFromUnSch(prozewo);

                    if (alSch.Count > 0)
                    {
                        Thread.Sleep(100);
                        doInsertT2ProcurementOperations(alSch, prozewo);

                        ArrayList al99 = getAndUpdateSRTKeys(alSch);
                        if (al99.Count > 0)
                        {
                            updateSrtKeyInProcurementOpeartions(al99);
                        }
                    }


                   
                    
                    /* for (int i = 0; i < alSch.Count; i++)
                     {
                         UnSchDao dao = (UnSchDao)alSch[i];
                         doInsertT2ProcurementOperations(dao, prozewo);
                     }*/

                }//

                // update
                log.Info(" All done \n\n");
            }
            else
            {
                log.Info("No rows found \n\n");
            }
        }

        private static String MATL_SQL = "select  distinct(prod_zewo) from t2_material m  WITH (NOLOCK) " +
            " where m.material_status='In-Progress' and " + //m.material_status='In-Progress'  and 
            " len(m.prod_zewo)>0  and m.routing_scope in (\'In-House\',\'Turn Key\', \'Farm-Out\') and m.routing_status=\'Completed\' ";// +
         //" and prod_ZEWO = \'107562131\' ";

        private static ArrayList getAllTPCMaterials()
        {
            ArrayList al = new ArrayList();
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    log.Info("getAllTPCMaterials() query : MATL_SQL = " + MATL_SQL);

                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(MATL_SQL, cnn))
                    {
                        cmd.CommandType = CommandType.Text;
                        
                        //cmd.Parameters.Add(new SqlParameter("@ACTION", action));

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                String prod_zeo = "";
                                if ((reader["prod_ZEWO"]) != DBNull.Value)
                                {
                                    prod_zeo = (String)reader["prod_ZEWO"];
                                    al.Add(prod_zeo.Trim());
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                log.Error("Error in getting getAllTPCMaterials() " + ee.Message);
            }
            return al;
        }


        private static String SQL_UNSCH_SQL = "";// "select  * from t_me2n where pr_num =@PR_NUM  and pr_item_num=@PR_ITEM_NUM;";
        private static ArrayList getAllOperationsFromUnSch(String prodzewo)
        {

            SQL_UNSCH_SQL = "select  * from t_unsch  WITH (NOLOCK) where mfg_order like '%" + prodzewo.Trim() + "%'";

            ArrayList al = new ArrayList();
            using (SqlConnection con = new SqlConnection(MYGlobal.getCString()))
            {
                con.Open();                

                using (SqlCommand cmd = new SqlCommand(SQL_UNSCH_SQL, con))
                {
                    cmd.CommandType = CommandType.Text;
                    // cmd.Parameters.AddWithValue("@PR_NUM", "00"+prnum);
                    //cmd.Parameters.AddWithValue("@PR_ITEM_NUM", pritemnum);

                    log.Info("  Unsch sql =" + SQL_UNSCH_SQL);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            try
                            {
                                UnSchDao dao = new UnSchDao();
                                dao.Plant = (string)reader["PLANT"];
                                // dao.Id = (Int32)reader["id"];

                                if (reader["UnConfirm_WorkCenter"] != DBNull.Value)
                                {
                                    //if no Po-num dont add it
                                    dao.UnConfirmWC = (String)reader["UnConfirm_WorkCenter"];
                                    //log.Info("prodzewo:" + prodzewo + "  >  Got UnConfirmWC : " + dao.UnConfirmWC);
                                }
                                else
                                {
                                    continue;
                                }

                                if (reader["oper_id"] != DBNull.Value)
                                {
                                    dao.OperationId = (String)reader["oper_id"];
                                }
                                else
                                {
                                    continue;
                                }

                                if (reader["current_oper"] != DBNull.Value)
                                {
                                    dao.CurrentOper = (Int32)reader["current_oper"];
                                }


                                if (reader["oper"] != DBNull.Value)
                                {
                                    dao.Oper = (Int32)reader["oper"];
                                }

                                if (reader["Order_Qty"] != DBNull.Value)
                                {
                                    dao.Qty = (int)reader["Order_Qty"];
                                }


                                if (reader["resource"] != DBNull.Value)
                                {
                                    dao.ResourceWC = (String)reader["resource"];
                                }

                                if (reader["resource_descrition"] != DBNull.Value)
                                {
                                    dao.ResourceDesc = (String)reader["resource_descrition"];
                                }

                                if (reader["Run_time_Hrs"] != DBNull.Value)
                                {
                                    dao.RunTimeHrs = (double)reader["Run_time_Hrs"];
                                }

                                if (reader["mfg_order"] != DBNull.Value)
                                {
                                    dao.MfgOrder = (String)reader["mfg_order"];
                                }



                                //----- add ---
                                al.Add(dao);

                            }
                            catch (Exception ee)
                            {
                                log.Error("Error in getAllUnSch : " + ee.Message);
                            }


                        }
                    }
                }
            }

            return al;
        }


        private static void deleteProcOperations(String prod_zewo)
        {
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    
                        try
                        {
                            string sql = string.Empty;                            
                          
                            sql = "delete t2_proc_operations where prod_zewo='" + prod_zewo + "'";
                            log.Info("deleteProcOperations sql = "+sql);

                            using (SqlCommand cmd = new SqlCommand(sql, cnn))
                            {
                                cmd.CommandType = CommandType.Text;
                                // log.Info("updateSrtKeyInProcurementOpeartions() queryuy  = " + sql);

                                int affRows = cmd.ExecuteNonQuery();
                                // log.Info("Affrows = " + affRows);
                            }
                        }
                        catch (Exception ee)
                        {
                            log.Error("deleteProcOperations() = " + ee.Message);
                        }
                     
                }
            }
            catch (Exception ee)
            {

                log.Error("Error in deleteProcOperations() = " + ee.Message + "\n\n");
            }
        }


        //mn=103075890, zewo
        public static void doInsertT2ProcurementOperations(ArrayList unschDaoList, String prod_zewo)
        {
            int affRows = 0;

            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();
                    try
                    {
                        ArrayList alx = new ArrayList();
                        for (int x = 0; x < unschDaoList.Count; x++)
                        {

                            UnSchDao dao = (UnSchDao)unschDaoList[x];

                            try
                            {
                                //delete 
                                if (!alx.Contains(prod_zewo))
                                {
                                    deleteProcOperations(prod_zewo);
                                    alx.Add(prod_zewo);
                                }
                            }
                            catch (Exception ee)
                            {
                                log.Error(" error in deleting : " + ee.Message);
                            }

                        }

                        alx.Clear();
                        alx = null;
                    }catch(Exception ee)
                    {
                        log.Error(" "+ ee.Message);
                    }

                    Thread.Sleep(10);

                    for (int x = 0; x < unschDaoList.Count; x++)
                    {

                        UnSchDao dao = (UnSchDao)unschDaoList[x];
                        //insert
                        using (SqlCommand cmd = new SqlCommand("[SP_INSERT_UPDATE_PROCUREMENT_OPERATIONS]", cnn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            if (dao.Id > 0)
                            {
                                cmd.Parameters.AddWithValue("@ID", dao.Id);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@ID", 0);
                            }


                            if (dao.ResourceWC.Contains("SO"))
                            {
                                if (!dao.ResourceWC.Equals("2088-SOTPCWH0"))
                                {
                                    try
                                    {
                                        cmd.Parameters.AddWithValue("@PROD_ZEWO", prod_zewo);
                                        cmd.Parameters.AddWithValue("@OPERATION_NUM", dao.Oper);
                                        cmd.Parameters.AddWithValue("@OPERATION", dao.OperationId+"");
                                        cmd.Parameters.AddWithValue("@RESOURCE_WC", dao.ResourceWC);
                                        cmd.Parameters.AddWithValue("@RESOURCE_WC_DESC", dao.ResourceDesc);
                                        cmd.Parameters.AddWithValue("@PROC_STATUS", "New");
                                        cmd.Parameters.AddWithValue("@INSERT_DATE", DateTime.Now.ToString("yyyy-MM-dd"));


                                        //exe
                                        affRows = cmd.ExecuteNonQuery();
                                        log.Info(" Insert procurement Success, Affrows = " + affRows + ", prod_zewo="+ prod_zewo +"\n\n");
                                    }catch(Exception ee)
                                    {
                                        log.Error("Exception = " + ee.Message);
                                    }
                                }
                            }
                            else
                            {
                                log.Info("Not a Procurement Operation " + dao.ResourceWC);
                            }

                        }

                    }
                }
            }
            catch (Exception ee)
            {
                log.Info("Error in saving procurement=> " + ee.Message);
            }
        }


        /**
         * select top 10  pordop.OPR_NBR, pordop.dsc, pordop.OPR_QTY,  pordop.PRCH_INFO_RCRD_NBR, pordop.SRT_KEY, pordop.PRCH_RQSTN_NBR,
            pord.PROD_ORDR_NBR from PROD_ORDR_OPR pordop , PROD_ORDR pord 
                where pordop.PROD_ORDR_OID=pord.OID and  pord.PROD_ORDR_NBR like '%106898643' and pordop.OPR_NBR=20;
         * 
         * 1. Get SRT Key from AP1MFGReporting
         * 2. Update SRT key in the app
         */
        public static ArrayList  getAndUpdateSRTKeys(ArrayList unschDaoList)
        {

            ArrayList al = new ArrayList();

            using (SqlConnection con = new SqlConnection(MYGlobal.getAP1DBCStringAP1MPM()))
            {
                con.Open();

                if(unschDaoList.Count > 0)
                {

                    for(int x=0;x< unschDaoList.Count; x++)
                    {
                        UnSchDao dao = (UnSchDao)unschDaoList[x];
                        string prodzewo = dao.MfgOrder;
                        int oper_id = dao.Oper;

                        string _SQL = " select   pordop.OPR_NBR, pordop.dsc, pordop.OPR_QTY,  pordop.PRCH_INFO_RCRD_NBR, pordop.SRT_KEY, pordop.PRCH_RQSTN_NBR, " +
                            "  pord.PROD_ORDR_NBR ,pordop.DEL_FLG  " +
                            " from PROD_ORDR_OPR pordop  WITH (NOLOCK) , PROD_ORDR pord   WITH (NOLOCK)       " +
                            "       where pordop.PROD_ORDR_OID = pord.OID and pord.PROD_ORDR_NBR like '%"+ prodzewo + "%' and pordop.OPR_NBR ="+ oper_id;


                        log.Info("getAndUpdateSRTKeys() sql = "+_SQL);

                        using (SqlCommand cmd = new SqlCommand(_SQL, con))
                        {
                            cmd.CommandType = CommandType.Text;
                           
                            //log.Info(" getAndUpdateSRTKeys()  _SQL  =" + _SQL);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {

                                while (reader.Read())
                                {
                                    try
                                    {                               
                                       string srt_key = string.Empty;
                                        bool del = false;

                                        if (reader["SRT_KEY"] != DBNull.Value)
                                        {                                          
                                            srt_key = (String)reader["SRT_KEY"];                                          
                                        }

                                        if (reader["DEL_FLG"] != DBNull.Value)
                                        {
                                            del = (bool)reader["DEL_FLG"];
                                        }

                                        log.Info("zewo =" + prodzewo + " srtkey = " + srt_key);

                                        ProcurementOperDao pdao = new ProcurementOperDao();
                                        pdao.ProdZEWO = prodzewo.Substring(3);//remove first 3 000
                                        pdao.Operation = dao.OperationId;// +": [SRTKey="+ srt_key+"]";
                                        pdao.OperNo = dao.Oper;
                                        pdao.SRTKey = srt_key;
                                        pdao.Deleted = del;


                                        al.Add(pdao);
                                    }
                                    catch (Exception ee)
                                    {
                                        log.Error("Error in getAllUnSch : " + ee.Message);
                                    }

                                }
                            }

                        }
                    }
                }
            }

            return al;

        }


        
        private static void updateSrtKeyInProcurementOpeartions(ArrayList al)
        {
            try
            {
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
                {
                    cnn.Open();

                    for(int x = 0; x < al.Count; x++)
                    {
                        ProcurementOperDao pdao = (ProcurementOperDao)al[x];

                        try
                        {
                            string sql = string.Empty;
                            if (pdao.Deleted)
                            {
                                sql = "delete from t2_proc_operations   where prod_zewo='" + pdao.ProdZEWO + "' and oper_nbr=" + pdao.OperNo;
                            }
                            else
                            {
                                sql = "update t2_proc_operations set srt_key='" + pdao.SRTKey + "' where prod_zewo='" + pdao.ProdZEWO + "' and oper_nbr=" + pdao.OperNo;
                            }
                            
                            
                            using (SqlCommand cmd = new SqlCommand(sql, cnn))
                            {
                                cmd.CommandType = CommandType.Text;
                               // log.Info("updateSrtKeyInProcurementOpeartions() queryuy  = " + sql);


                                int affRows = cmd.ExecuteNonQuery();
                               // log.Info("Affrows = " + affRows);
                            }
                        }catch(Exception ee)
                        {
                            log.Error("updateSrtKeyInProcurementOpeartions() = " + ee.Message);
                        }
                    }
                }
            }
            catch (Exception ee)
            {

                log.Error("Error in updateSrtKeyInProcurementOpeartions() = " + ee.Message + "\n\n");
            }
        }
         

    }
}
