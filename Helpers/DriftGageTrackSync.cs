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
* 
* THis app will sync drift gage db with gage track 
*/
namespace TPC2UpdaterApp.Helpers
{
    public class DriftGageTrackSync
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static ArrayList error_list = new ArrayList();


        public DriftGageTrackSync()
        {

        }


        public static void doAllGageTrackUpdate()
        {
            ArrayList al = doGetAllGageTrack();

            DateTime current_time = DateTime.Now;
              using (SqlConnection cnn = new SqlConnection(MYGlobal.getSing3MEIECString()))
            {
                cnn.Open();
                for (int x = 0; x < al.Count; x++)
                {
                    GageTrackMasterDao dao = (GageTrackMasterDao)al[x];
                      doUpdateGageTools(dao, cnn, x);
                }
                log.Info("All update donw \n\n");

                if (error_list.Count > 0)
                {
                    for (int x = 0; x < error_list.Count; x++)
                    {
                        GageTrackMasterDao dao = (GageTrackMasterDao)error_list[x];
                        doInsertNewRecords(dao, cnn);
                    }
                }

            }

            DateTime finish_time = DateTime.Now;
            TimeSpan duration = current_time - finish_time;
            double elapsed_seconds = duration.TotalMilliseconds;

            
            log.Info("Error list size " + error_list.Count  +", Finished in "+ elapsed_seconds +" seconds");

        }

        
        public static void doUpdateGageTools(GageTrackMasterDao dao, SqlConnection cnn, int row)
        {
            String sql = string.Empty;

            if (dao.CurrentLocation==null)
            {
                dao.CurrentLocation = "";
            }

            if (dao.StorageLocation==null)
            {
                dao.StorageLocation = "";
            }

            if (dao.CalibrationFrequency == 0 || dao.LastCalibDate==null || dao.NextDueDate==null)
            {
                //error_list.Add(dao);
                sql = "update t_tool set LOCATION='" + dao.StorageLocation + "', current_location='" + dao.CurrentLocation + "',"             
              + "', UPD_ON='" + DateTime.Now + "' where TOOL_ID='" + dao.GageId + "'";
                return;
            }
            else
            {
                sql = "update t_tool set LOCATION='" + dao.StorageLocation + "', current_location='" + dao.CurrentLocation + "'," +
              " calibration_date='" + dao.LastCalibDate.ToString("yyyy-MM-dd") + "', next_calib_date='" + dao.NextDueDate.ToString("yyyy-MM-dd")
              + "', UPD_ON='" + DateTime.Now + "' where TOOL_ID='" + dao.GageId + "'";
            }

            
            try
            {

                log.Info("update sql= "+sql);

                using (SqlCommand cmd = new SqlCommand(sql, cnn))
                {
                    cmd.CommandType = CommandType.Text;

                    int affRows = cmd.ExecuteNonQuery();
                    log.Info("Row="+ row + ", update success Affrows = " + affRows + ", gageid=" + dao.GageId);

                    if (affRows == 0)
                    {
                        //record not found
                        error_list.Add(dao);
                    }
                }
            }catch(Exception ee)
            {
                log.Error("doUpdateGageTools() "+ "Row=" + row +", vSome error may be not in the db " + ee.Message +", Gageid="+dao.GageId + ", CurrentLocation=" + dao.CurrentLocation);
               // error_list.Add(dao);
            }
        }


        private static void doInsertNewRecords(GageTrackMasterDao dao, SqlConnection cnn)
        {
            string sql = string.Empty;


            if (dao.CalibrationFrequency == 0 || dao.LastCalibDate.Year<2000 || dao.NextDueDate.Year < 2000)
            {
                sql = "INSERT INTO [t_tool] ([TOOL_ID],[TOOL_SHORT_DESC],[TOOL_LONG_DESC],[LOCATION],[STATUS]," +
               "[DESC_1],[DESC_2],[DESC_3],[DESC_4],[DESC_5],[PLANT],[storage_location]," +
               "[current_location],[category] ,[sub_category],[UPD_ON]) values (@TOOL_ID, @TOOL_SHORT_DESC, @TOOL_LONG_DESC, @LOCATION," +
               "@STATUS, @DESC_1, @DESC_2, @DESC_3, @DESC_4, @DESC_5, @PLANT, @storage_location," +
               " @current_location, @category, @sub_category, @UPD_ON )";
            }
            else
            {
                sql = "INSERT INTO [t_tool] ([TOOL_ID],[TOOL_SHORT_DESC],[TOOL_LONG_DESC],[LOCATION],[STATUS]," +
               "[DESC_1],[DESC_2],[DESC_3],[DESC_4],[DESC_5],[PLANT],[calibration_date],[next_calib_date],[storage_location]," +
               "[current_location],[category] ,[sub_category],[UPD_ON]) values (@TOOL_ID, @TOOL_SHORT_DESC, @TOOL_LONG_DESC, @LOCATION," +
               "@STATUS, @DESC_1, @DESC_2, @DESC_3, @DESC_4, @DESC_5, @PLANT, @calibration_date, @next_calib_date, @storage_location," +
               " @current_location, @category, @sub_category, @UPD_ON )";
            }

                try
            {
                using (SqlCommand cmd = new SqlCommand(sql, cnn))
                {
                    cmd.CommandType = CommandType.Text;

                    if (dao.UserRef1==null)
                    {
                        dao.UserRef1 = "";
                    }
                    if (dao.UserRef2 == null)
                    {
                        dao.UserRef2 = "";
                    }

                    if (dao.UserRef3 == null)
                    {
                        dao.UserRef3 = "";
                    }

                    if (dao.UserRef4 == null)
                    {
                        dao.UserRef4 = "";
                    }

                    if (dao.UserRef5 == null)
                    {
                        dao.UserRef5 = "";
                    }

                    if (dao.StorageLocation == null)
                    {
                        dao.StorageLocation = "";
                    }
                    if (dao.CurrentLocation == null)
                    {
                        dao.CurrentLocation = "";
                    }
                    if (dao.StandaardGroup == null)
                    {
                        dao.StandaardGroup = "";
                    }
                    if (dao.GMType == null)
                    {
                        dao.GMType = "";
                    }
                    cmd.Parameters.AddWithValue("@TOOL_ID", dao.GageId);
                    cmd.Parameters.AddWithValue("@TOOL_SHORT_DESC", dao.GageDescription);
                    cmd.Parameters.AddWithValue("@TOOL_LONG_DESC", "");
                    cmd.Parameters.AddWithValue("@LOCATION", dao.CurrentLocation);
                    cmd.Parameters.AddWithValue("@STATUS", dao.Status);
                    cmd.Parameters.AddWithValue("@DESC_1", dao.UserRef1);
                    cmd.Parameters.AddWithValue("@DESC_2", dao.UserRef2);
                    cmd.Parameters.AddWithValue("@DESC_3", dao.UserRef3);
                    cmd.Parameters.AddWithValue("@DESC_4", dao.UserRef4);

                    cmd.Parameters.AddWithValue("@DESC_5", dao.UserRef5);
                    cmd.Parameters.AddWithValue("@PLANT", "2088");
                    if (dao.CalibrationFrequency == 0 || dao.LastCalibDate.Year < 2000 || dao.NextDueDate.Year < 2000)
                    {
                       // cmd.Parameters.AddWithValue("@calibration_date", null);
                       // cmd.Parameters.AddWithValue("@next_calib_date", null);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@calibration_date", dao.LastCalibDate);
                        cmd.Parameters.AddWithValue("@next_calib_date", dao.NextDueDate);
                    }
                   
                    cmd.Parameters.AddWithValue("@storage_location", dao.StorageLocation);
                    cmd.Parameters.AddWithValue("@current_location", dao.CurrentLocation);
                    cmd.Parameters.AddWithValue("@sub_category", dao.StandaardGroup);
                    cmd.Parameters.AddWithValue("@category", dao.GMType);
                    cmd.Parameters.AddWithValue("@UPD_ON", DateTime.Now);


                    int affRows = cmd.ExecuteNonQuery();
                    log.Info("Inser success Affrows = " + affRows + ", gageid=" + dao.GageId);

                    if (affRows == 0)
                    {
                        //record not found
                        log.Info("Insert failed ");
                    }
                }
            }
            catch (Exception ee)
            {
                log.Error("doInsertNewRecords() Some error may be not in the db " + ee.Message + ", Gageid=" + dao.GageId + ", CurrentLocation=" + dao.CurrentLocation);
                //error_list.Add(dao);
            }

        }



        //SELECT  *  FROM [GAGETRAK70].[GAGEMGR65].[Gage_Master]
        //SELECT  *  FROM [SING3_MEIE].[dbo].[t_tool]
        /**
         *  Get all GageTrack records and update one by one in the drift db
         * SELECT  Gage_ID, Description, GM_Type, Unit_of_Meas, Standard_Group, Storage_Location, Current_Location, 
Calibration_Frequency, Calibration_Frequency_UOM, Next_Due_Date, Last_Calibration_Date, Status, UserDef1,
UserDef2, UserDef3, UserDef4  FROM [GAGEMGR65].[Gage_Master]--order by Gage_ID where Gage_ID='123377';
         */
        private static ArrayList doGetAllGageTrack()
        {
            ArrayList al = new ArrayList();

            string sql = " SELECT   Gage_ID, Description, GM_Type, Unit_of_Meas, Standard_Group, Storage_Location, Current_Location, " +
                " Calibration_Frequency, Calibration_Frequency_UOM, Next_Due_Date, Last_Calibration_Date, Status, UserDef1,UserDef2, UserDef3, UserDef4, UserDef5 " +
                " FROM [GAGEMGR65].[Gage_Master] WITH (NOLOCK) ";
              try
            {
                using (SqlConnection cnn = new SqlConnection(DBUtils.getGageTrackDbCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        cmd.CommandType = CommandType.Text;
                        log.Info(" doGetAllGageTrack() sql=" + sql);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                GageTrackMasterDao dao = new GageTrackMasterDao();
                               
                                //int id = (Int32)reader["Id"];
                                if ((reader["Gage_ID"]) != DBNull.Value)
                                {
                                    dao.GageId = (String)reader["Gage_ID"];
                                }

                                if ((reader["Description"]) != DBNull.Value)
                                {
                                    dao.GageDescription = (String)reader["Description"];
                                }

                                if ((reader["GM_Type"]) != DBNull.Value)
                                {
                                    dao.GMType = (String)reader["GM_Type"];
                                }

                                if ((reader["Unit_of_Meas"]) != DBNull.Value)
                                {
                                    dao.Unit = (String)reader["Unit_of_Meas"];
                                }

                                if ((reader["Standard_Group"]) != DBNull.Value)
                                {
                                    dao.StandaardGroup = (String)reader["Standard_Group"];
                                }

                                if ((reader["Storage_Location"]) != DBNull.Value)
                                {
                                    dao.StorageLocation = (String)reader["Storage_Location"];
                                }

                                if ((reader["Current_Location"]) != DBNull.Value)
                                {
                                    dao.CurrentLocation = (String)reader["Current_Location"];
                                }
                                if ((reader["Calibration_Frequency"]) != DBNull.Value)
                                {
                                    dao.CalibrationFrequency = (double)reader["Calibration_Frequency"];
                                }
                                if ((reader["Calibration_Frequency_UOM"]) != DBNull.Value)
                                {
                                    dao.CablicarationUO = (String)reader["Calibration_Frequency_UOM"];
                                }
                                if ((reader["Next_Due_Date"]) != DBNull.Value)
                                {
                                    dao.NextDueDate = (DateTime)reader["Next_Due_Date"];
                                }
                                if ((reader["Last_Calibration_Date"]) != DBNull.Value)
                                {
                                    dao.LastCalibDate = (DateTime)reader["Last_Calibration_Date"];
                                }
                                if ((reader["Status"]) != DBNull.Value)
                                {
                                    dao.Status = (String)reader["Status"];
                                }
                                if ((reader["UserDef1"]) != DBNull.Value)
                                {
                                    dao.UserRef1 = (String)reader["UserDef1"];
                                }
                                if ((reader["UserDef2"]) != DBNull.Value)
                                {
                                    dao.UserRef2 = (String)reader["UserDef2"];
                                }
                                if ((reader["UserDef3"]) != DBNull.Value)
                                {
                                    dao.UserRef3 = (String)reader["UserDef3"];
                                }
                                if ((reader["UserDef4"]) != DBNull.Value)
                                {
                                    dao.UserRef4 = (String)reader["UserDef4"];
                                }

                                if ((reader["UserDef5"]) != DBNull.Value)
                                {
                                    dao.UserRef5 = (String)reader["UserDef5"];
                                }


                                al.Add(dao);
                            }//while
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                log.Error("Error in getting doGetAllGageTrack  " + ee.Message);
            }

            return al;
        }

        //use single connection


    }
}
