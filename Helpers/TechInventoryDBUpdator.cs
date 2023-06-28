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
 * Added in GlobalReportHelper
 * 
 */
namespace TPC2UpdaterApp.Helpers
{
    public class TechInventoryDBUpdator
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /**
        * 1. Get all project in 2021
        * 2. Get all related materials.
        * 3. update project & material in TechDB
        * 
        */
        public static void doTechInventory()
        {
           log.Info("year format = "+ getYearFormat());

            ArrayList matr_insert_list = new ArrayList();
            //1. Get all TPC Projects
            ArrayList proj_list = getAllTPCProjects();
            for(int x=0;x< proj_list.Count; x++)
            {
                T2ProjectDao dao = (T2ProjectDao)proj_list[x];
                //2. Check Project exists in  TechDB
               
                TechProjectDao tpdao = doCheckTechDBProjectExists(dao.TrackingNumber);
                if (tpdao==null)
                {
                    //specail attribute                   
                    matr_insert_list.Add(dao);
                }
            }


            for (int x = 0; x < matr_insert_list.Count; x++)
            {
                T2ProjectDao dao = (T2ProjectDao)matr_insert_list[x];
                //insert tech project
                bool proj_insert_status =  doInsertTechDBProject(dao);
                if (proj_insert_status)
                {

                    //get the proejct we need proejct ID
                    TechProjectDao tpdao2 = doCheckTechDBProjectExists(dao.TrackingNumber);


                    ArrayList mtlist = getAllTPCMaterialList(dao.Id);
                    //insert tech material
                    for(int j=0;j< mtlist.Count; j++)
                    {
                        T2MaterialDao mdao = (T2MaterialDao)mtlist[j];
                        bool bbx = doInsertTechDBMaterial(mdao, dao.SubmittedBy, tpdao2.Id);
                        if (bbx)
                        {
                            log.Info("Material Insert success");
                        }
                        else
                        {
                            log.Info("Material Insert failed");
                        }
                    }               

                }               
            }
        }


        private static TechProjectDao doCheckTechDBProjectExists(string trackingNum)
        {
            TechProjectDao dao = null;
            string sql = "select * from t_project WITH (NOLOCK)  where tracking_number='" + trackingNum + "'";
            using (SqlConnection cnn = new SqlConnection(MYGlobal.getTechDBCString()))
            {
                cnn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, cnn))
                {
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                dao = new TechProjectDao();
                                dao.TrackingNum = (string)reader["tracking_number"];
                                dao.Id = (int)reader["id"];
                                dao.NetworkId = (string)reader["network_id"];
                                dao.ProjectGroup = (string)reader["project_group"];

                                return dao;
                            }catch(Exception ee)
                            {
                                log.Error("EE " + ee);
                            }
                        }
                    }
                }
            }

            return dao;
        }

        private static bool doInsertTechDBProject(T2ProjectDao dao)
        {
            string sql = "insert into t_project(network_id, project_group, tracking_number, project_type, actvity_code, project_manager, master_zewo, project_engineer, sub_psl, added_by, added_on, is_tpc, inv_owner ) " +
                "values (@network_id, @project_group, @tracking_number, @project_type, @actvity_code, @project_manager,  @master_zewo, @project_engineer, @sub_psl, @added_by, @added_on, @is_tpc, @inv_owner)";
            using (SqlConnection cnn = new SqlConnection(MYGlobal.getTechDBCString()))
            {
                cnn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, cnn))
                {
                    try
                    {
                        log.Info("doInsertTechDBProject() sql = " + sql +" , Tracking num="+ dao.TrackingNumber);
                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.AddWithValue("@network_id", dao.SAPNetwork);
                        cmd.Parameters.AddWithValue("@project_group", dao.ProjectName);
                        cmd.Parameters.AddWithValue("@tracking_number", dao.TrackingNumber);
                        cmd.Parameters.AddWithValue("@project_type", dao.FormType);
                        cmd.Parameters.AddWithValue("@actvity_code", dao.ActCode);
                        cmd.Parameters.AddWithValue("@project_manager", dao.TechProjMgr);
                        cmd.Parameters.AddWithValue("@master_zewo", dao.SingZewo);
                        cmd.Parameters.AddWithValue("@project_engineer", dao.TechRespEngr);
                        cmd.Parameters.AddWithValue("@sub_psl", dao.SubPsl);

                        cmd.Parameters.AddWithValue("@added_by", dao.SubmittedBy);
                        cmd.Parameters.AddWithValue("@added_on", dao.EntryDate);

                        cmd.Parameters.AddWithValue("@is_tpc", 1);
                        cmd.Parameters.AddWithValue("@inv_owner", 1);

                        int rows = cmd.ExecuteNonQuery();
                        log.Info("doInsertTechDBProject   Affected rows " + rows);
                        return true;
                    }catch(Exception ee)
                    {
                        log.Error("Error while inserting project = "+ee.Message);
                        
                    }
                }
            }

            return false;
        }


        private static bool doInsertTechDBMaterial(T2MaterialDao dao, string by, int techProjectId)
        {

            if (dao.MaterialNbr.Equals("00000"))
            {
                return false;
            }

            try
            {
                string sql = "insert into t_material_master(part_number, drawing_number, description, remarks, added_on, " +
                    "added_by, size, unit, project_id, drawing_rev, material_rev, material_spec, raw_material, project_qty) " +
                    " values(@part_number, @drawing_number, @description, @remarks, @added_on, @added_by, " +
                    " @size, @unit, @project_id, @drawing_rev, @material_rev, @material_spec, @raw_material, @project_qty) ";
                using (SqlConnection cnn = new SqlConnection(MYGlobal.getTechDBCString()))
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, cnn))
                    {
                        log.Info("doInsertTechDBMaterial() sql = " + sql);
                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.AddWithValue("@part_number", dao.MaterialNbr);
                        cmd.Parameters.AddWithValue("@drawing_number", dao.DrawingNbr);
                        cmd.Parameters.AddWithValue("@description", dao.MaterialDesc);
                        cmd.Parameters.AddWithValue("@remarks", "");
                        cmd.Parameters.AddWithValue("@added_on", DateTime.Now);
                        cmd.Parameters.AddWithValue("@added_by", by);
                        cmd.Parameters.AddWithValue("@size", "");
                        if (dao.MaterialUnit.Length < 1)
                        {
                            cmd.Parameters.AddWithValue("@unit", "");
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@unit", dao.MaterialUnit);
                        }

                        cmd.Parameters.AddWithValue("@project_id", techProjectId);
                        cmd.Parameters.AddWithValue("@drawing_rev", dao.DrawingRev);
                        cmd.Parameters.AddWithValue("@material_rev", dao.MaterialRev);
                        cmd.Parameters.AddWithValue("@material_spec", dao.MaterialSpec);
                        cmd.Parameters.AddWithValue("@raw_material", dao.RawMaterial);
                        cmd.Parameters.AddWithValue("@project_qty", dao.TotalReqQty);

                        int rows = cmd.ExecuteNonQuery();
                        log.Info("doInsertTechDBMaterial   Affected rows " + rows);
                        return true;
                    }
                }
            }catch(Exception ee)
            {
                log.Error("Error " + ee.Message);
            }

            return false;
        }



        public static ArrayList getAllTPCMaterialList(int projectId)
        {
            ArrayList al = new ArrayList();

            string sql = "select id, material_num, total_req_qty,material_desc,raw_material, material_spec, drawing_num, drawing_revision, material_unit," +
                " project_id, material_rev  from t2_material WITH (NOLOCK)  where  project_id=" + projectId + "   order by id desc";

            using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
            {
                cnn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, cnn))
                {
                    log.Info("getAllMaterialList() sql = " + sql);
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            T2MaterialDao dao = new T2MaterialDao();

                            dao.Id = (Int32)reader["id"];
                            dao.MaterialNbr = (string)reader["material_num"];
                            dao.ProjectId = (int)reader["project_id"];

                            if ((reader["total_req_qty"]) != DBNull.Value)
                            {
                                dao.TotalReqQty = (int)reader["total_req_qty"];
                            }
                            if ((reader["material_desc"]) != DBNull.Value)
                            {
                                dao.MaterialDesc = (string)reader["material_desc"];
                            }

                            if ((reader["raw_material"]) != DBNull.Value)
                            {
                                dao.RawMaterial = (string)reader["raw_material"];
                            }

                            if ((reader["material_spec"]) != DBNull.Value)
                            {
                                dao.MaterialSpec = (string)reader["material_spec"];
                            }

                            if ((reader["drawing_num"]) != DBNull.Value)
                            {
                                dao.DrawingNbr = (string)reader["drawing_num"];
                            }

                            if ((reader["drawing_revision"]) != DBNull.Value)
                            {
                                dao.DrawingRev = (string)reader["drawing_revision"];
                            }

                            if ((reader["material_unit"]) != DBNull.Value)
                            {
                                dao.MaterialUnit = (string)reader["material_unit"];
                            }

                            if ((reader["material_rev"]) != DBNull.Value)
                            {
                                dao.MaterialRev = (string)reader["material_rev"];
                            }


                            al.Add(dao);
                        }
                    }
                }
            }

            return al;
        }


        private static string getYearFormat()
        {
           return "S"+ DateTime.Now.ToString("yy")+"-";
        }



        static string current_year_sql = "select top 30 * from t2_project  WITH (NOLOCK)  where plant='2088' and tracking_number like '" + getYearFormat() + "%' " +
                " and project_status in ('Completed', 'In-Progress')   order by id desc";

        static string special_sql = "select top 30 * from t2_project  WITH (NOLOCK)  where  tracking_number in ( 'S21-0106','S21-0246','S21-0208','S21-0299','S21-0335','S21-0439','S21-0275','S21-0279') " +
                " and project_status in ('Completed', 'In-Progress')   order by id desc";

        

        //remove id from sql
        public static ArrayList getAllTPCProjects()
        {
            ArrayList al = new ArrayList();

            string sql = special_sql;

            using (SqlConnection cnn = new SqlConnection(MYGlobal.getCString()))
            {
                cnn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, cnn))
                {
                    log.Info("Project sql = " + sql);
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            T2ProjectDao pdao = new T2ProjectDao();

                            pdao.Id = (Int32)reader["id"];
                            pdao.ProjectName = (string)reader["project_name"];
                            pdao.PSLFamily = (string)reader["psl_family"];
                            pdao.SubPsl = (string)reader["sub_psl"];
                            pdao.SingZewo = (string)reader["sing_zewo"];
                            pdao.FormType = (string)reader["form_type"];
                            pdao.SAPNetwork = (string)reader["sap_network"];
                            pdao.ActCode = (string)reader["activity_code"];
                            pdao.TechProjMgr = (string)reader["tech_project_mgr"];
                            pdao.TechRespEngr = (string)reader["tech_resp_engineer"];

                            if ((reader["entry_date"]) != DBNull.Value)
                            {
                                pdao.EntryDate = (DateTime)reader["entry_date"];
                            }

                            pdao.TrackingNumber = (string)reader["tracking_number"];
                            pdao.Plant = (string)reader["plant"];
                            pdao.ProjectStatus = (string)reader["project_status"];

                            pdao.SubmittedBy = (string)reader["submitted_by"];

                            al.Add(pdao);
                        }
                    }
                }
            }

            return al;
        }

    }
}
