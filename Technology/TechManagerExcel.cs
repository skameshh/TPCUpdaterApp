using Microsoft.Office.Interop.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TPC2UpdaterApp.DB;


/**
 * its in TechExcelJob - 5.30am
 * 2 years data only
 * 
 */
namespace TPC2UpdaterApp.Helpers
{
    class TechManagerExcel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static  string SQL = "select material_id, entry_date, start_date, psl_family, sub_psl, sing_zewo, sap_network,  activity_code, tech_project_mgr," +
            " tech_resp_engineer, project_name, routing_scope, tracking_number, material_num,  material_desc, total_req_qty, promise_date," +
            " need_date, tpc_crk_date_line, wc_unconfirm_wc, prod_ZEWO, RMl_TK_and_purch_part_po_ln,   material_status, project_status, act_compltd_date_from_SAP, " +
            " SUBSTRING(desc_1,5,10 ) as price, desc_2 as approx_comp_date  from view_t2_mfg_delivery WITH (NOLOCK)  " +
            " where  project_status in('In-Progress','Completed') and  psl_family='HCT_Singapore' and  start_date > DATEADD(year, -3, getdate()) order by id desc ";


        public static void doTechMgrExcel()
        {
            ArrayList al = getAllProjectMaterials(SQL);
            doPrintProject(al);
        }


        private static ArrayList getAllProjectMaterials(String sql)
        {
            //2 years data only
           /* if (sql.Contains("where"))
            {
                sql = sql + " and p.start_date > DATEADD(year, -2, getdate()) ";
            }
            else
            {
                sql = sql + " where p.start_date > DATEADD(year, -2, getdate()) ";
            }*/


            ArrayList al = new ArrayList();
            using (SqlConnection sqlCon = new SqlConnection(MYGlobal.getCString()))
            {
                sqlCon.Open();
                log.Info(" getAllProjectMaterials()  sql = " + sql);
                using (SqlCommand cmd = new SqlCommand(sql, sqlCon))
                {
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            T2ProjectMaterialDao idao = new T2ProjectMaterialDao();
                            idao.MatlId = (Int32)reader["material_id"];
                            //idao.ProjectId = (Int32)reader["project_id"];   

                            //start
                            if ((reader["entry_date"]) != DBNull.Value)
                            {
                                idao.EntryDate = (DateTime)reader["entry_date"];
                            }
                             if (reader["start_date"] != DBNull.Value)
                             {
                                 idao.StartDate = (DateTime)reader["start_date"];
                             }
                            idao.PSLFamily = (string)reader["psl_family"];
                            idao.SubPsl = (string)reader["sub_psl"];
                            idao.SingZewo = (string)reader["sing_zewo"];
                            idao.SAPNetwork = (string)reader["sap_network"];
                            idao.ActCode = (string)reader["activity_code"];
                            idao.TechProjMgr = (string)reader["tech_project_mgr"];
                            idao.TechRespEngr = (string)reader["tech_resp_engineer"];
                            idao.ProjectName = (string)reader["project_name"];
                            if (reader["routing_scope"] != DBNull.Value)
                            {
                                idao.RoutingScope = (string)reader["routing_scope"];
                            }
                            idao.TrackingNumber = (string)reader["tracking_number"];
                            if (reader["material_num"] != DBNull.Value)
                            {
                                idao.MaterialNbr = (string)reader["material_num"];
                            }
                            if (reader["material_desc"] != DBNull.Value)
                            {
                                idao.MaterialDesc = (string)reader["material_desc"];
                            }
                            if (reader["total_req_qty"] != DBNull.Value)
                            {
                                idao.TotalReqQty = (int)reader["total_req_qty"];
                            }
                            //promise_date
                            if (reader["promise_date"] != DBNull.Value)
                            {
                                idao.WCPromizedDate = (DateTime)reader["promise_date"];
                            }

                            if (reader["need_date"] != DBNull.Value)
                            {
                                idao.NeedDate = (DateTime)reader["need_date"];
                            }
                            //tpc_crk_date_line
                            if (reader["tpc_crk_date_line"] != DBNull.Value)
                            {
                                idao.TPCCRKDateLine = (DateTime)reader["tpc_crk_date_line"];
                            }

                            /*if (reader["unconfirmed_wc"] != DBNull.Value)
                            {
                                idao.UNConfirmWC = (string)reader["unconfirmed_wc"];
                            }*/
                            if (reader["prod_ZEWO"] != DBNull.Value)
                            {
                                idao.ProdZEWO = (string)reader["prod_ZEWO"];
                            }
                            if (reader["material_status"] != DBNull.Value)
                            {
                                idao.MaterialStatus = (string)reader["material_status"];
                            }
                            idao.ProjectStatus = (string)reader["project_status"];

                            if (reader["act_compltd_date_from_SAP"] != DBNull.Value)
                            {
                                idao.ActCompltdDateFromSAP = (DateTime)reader["act_compltd_date_from_SAP"];
                            }
                            if (reader["RMl_TK_and_purch_part_po_ln"] != DBNull.Value)
                            {
                                idao.RMTKPUR_POLN = (string)reader["RMl_TK_and_purch_part_po_ln"];
                            }

                            if (reader["price"] != DBNull.Value)
                            {
                                idao.Price = (string)reader["price"];
                            }

                            //approx_comp_date
                            if (reader["approx_comp_date"] != DBNull.Value)
                            {
                                idao.ApproxComplDate = (String)reader["approx_comp_date"];
                            }

                            al.Add(idao);

                        }
                    }
                }
            }

                return al;
        }


        private static bool doDelete(String filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                
            }
            return true;
        }


        private static void doPrintProject(ArrayList al)
        {
            String name = "tech";
            // creating Excel Application  
            Microsoft.Office.Interop.Excel._Application app = new Microsoft.Office.Interop.Excel.Application();
            // creating new Excelsheet in workbook  
            Microsoft.Office.Interop.Excel._Worksheet worksheet = null;
            // creating new WorkBook within Excel application  
            Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);

            try
            {            
                
               
                // see the excel sheet behind the program  
                app.Visible = true;
                // get the reference of first sheet. By default its name is Sheet1.  
                // store its reference to worksheet  
                worksheet = workbook.Sheets["Sheet1"];
                worksheet = workbook.ActiveSheet;
                // changing the name of active sheet  
                worksheet.Name = "" + name;
                // storing header part in Excel  


                try
                {
                  

                   // worksheet.get_Range("A1", "AB100").AutoFormat(Microsoft.Office.Interop.Excel.XlRangeAutoFormat.xlRangeAutoFormatTable3,  Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,  Type.Missing);

                    /*   Microsoft.Office.Interop.Excel.Range myRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 28]];
                       object result = myRange.Table;*/

                    Microsoft.Office.Interop.Excel.Range SourceRange = worksheet.get_Range("A1", "AB"+al.Count);
                    string TableName = "Table1";
                    string TableStyleName ="tableStyle";
                    SourceRange.Worksheet.ListObjects.Add(XlListObjectSourceType.xlSrcRange, SourceRange, System.Type.Missing, XlYesNoGuess.xlYes, System.Type.Missing).Name =  TableName;
                    SourceRange.Select();
                    SourceRange.Worksheet.ListObjects[TableName].TableStyle = TableStyleName;

                }
                catch(Exception ee)
                {
                    log.Info("Errro " + ee.Message);
                }


                /*  try
                  {
                      Microsoft.Office.Interop.Excel.Range myRange = worksheet.Range["A1", "A1000"];
                      myRange.AutoFilter(1, "Apple", Microsoft.Office.Interop.Excel.XlAutoFilterOperator.xlAnd, true);
                  }
                  catch (Exception ee)
                  {
                      log.Info("Fitler Err : " + ee.Message);
                  }
                  */

                log.Info("Total rows " + al.Count +"\n\n");

                int rr = 1;
                int rrHelper = 1;
                worksheet.Cells[rr, rrHelper++] = "ID";
                worksheet.Cells[rr, rrHelper++] = "entry_date";
                worksheet.Cells[rr, rrHelper++] = "start_date";
                worksheet.Cells[rr, rrHelper++] = "psl_family";
                worksheet.Cells[rr, rrHelper++] = "sub_psl";
                worksheet.Cells[rr, rrHelper++] = "sing_zewo";
                worksheet.Cells[rr, rrHelper++] = "sap_network";
                worksheet.Cells[rr, rrHelper++] = "activity_code";
                worksheet.Cells[rr, rrHelper++] = "tech_project_mgr";
                worksheet.Cells[rr, rrHelper++] = "tech_resp_engineer";
                worksheet.Cells[rr, rrHelper++] = "project_name";

                worksheet.Cells[rr, rrHelper++] = "routing_scope";
                worksheet.Cells[rr, rrHelper++] = "tracking_number";
                worksheet.Cells[rr, rrHelper++] = "material_num";
                worksheet.Cells[rr, rrHelper++] = "material_desc";
                worksheet.Cells[rr, rrHelper++] = "total_req_qty";
                worksheet.Cells[rr, rrHelper++] = "promise_date";
                worksheet.Cells[rr, rrHelper++] = "need_date";
                worksheet.Cells[rr, rrHelper++] = "tpc_crk_date_line";
                worksheet.Cells[rr, rrHelper++] = "wc_unconfirm_wc";
                worksheet.Cells[rr, rrHelper++] = "prod_ZEWO";
                worksheet.Cells[rr, rrHelper++] = "RMl_TK_and_purch_part_po_ln";
                worksheet.Cells[rr, rrHelper++] = "material_status";
                worksheet.Cells[rr, rrHelper++] = "project_status";
                worksheet.Cells[rr, rrHelper++] = "act_compltd_date_from_SAP";               
                worksheet.Cells[rr, rrHelper++] = "price";
                worksheet.Cells[rr, rrHelper++] = "Approx_compl_date";

                

                // storing Each row and column value to excel sheet  
                for (int i = 1; i <= al.Count + 1; i++)
                {
                    T2ProjectMaterialDao dao = (T2ProjectMaterialDao)al[i - 1];
                    int rowhelper = 1;
                    //id
                    worksheet.Cells[i + 1, rowhelper++] = dao.MatlId;

                    if (dao.EntryDate != null)
                    {
                        if (dao.EntryDate.Year > 2000)
                        {
                            worksheet.Cells[i + 1, rowhelper++] = dao.EntryDate.ToString("yyyy-MMM-dd");
                        }
                        else
                        {
                            worksheet.Cells[i + 1, rowhelper++] = "";
                        }
                    }
                    else
                    {
                        worksheet.Cells[i + 1, rowhelper++] = "";
                    }

                    if (dao.StartDate != null)
                    {
                        if (dao.StartDate.Year > 2000)
                        {
                            worksheet.Cells[i + 1, rowhelper++] = dao.StartDate.ToString("yyyy-MMM-dd");
                        }
                        else
                        {
                            worksheet.Cells[i + 1, rowhelper++] = "";
                        }
                    }
                    else
                    {
                        worksheet.Cells[i + 1, rowhelper++] = "";
                    }

                    worksheet.Cells[i + 1, rowhelper++] = dao.PSLFamily;
                    worksheet.Cells[i + 1, rowhelper++] = dao.SubPsl;


                    worksheet.Cells[i + 1, rowhelper++] = dao.SingZewo;
                    worksheet.Cells[i + 1, rowhelper++] = dao.SAPNetwork;
                    worksheet.Cells[i + 1, rowhelper++] = dao.ActCode;
                    worksheet.Cells[i + 1, rowhelper++] = dao.TechProjMgr;
                    worksheet.Cells[i + 1, rowhelper++] = dao.TechRespEngr;
                    worksheet.Cells[i + 1, rowhelper++] = dao.ProjectName;

                    worksheet.Cells[i + 1, rowhelper++] = dao.RoutingScope;
                    worksheet.Cells[i + 1, rowhelper++] = dao.TrackingNumber;
                    worksheet.Cells[i + 1, rowhelper++] = dao.MaterialNbr;
                    worksheet.Cells[i + 1, rowhelper++] = dao.MaterialDesc;
                    worksheet.Cells[i + 1, rowhelper++] = dao.TotalReqQty;


                    if (dao.WCPromizedDate!= null){
                        
                        if (dao.WCPromizedDate.Year > 2000)
                        {
                            worksheet.Cells[i + 1, rowhelper++] = dao.WCPromizedDate.ToString("yyyy-MMM-dd");
                        }
                        else
                        {
                            worksheet.Cells[i + 1, rowhelper++] = "";
                        }
                    }
                    else
                    {
                        worksheet.Cells[i + 1, rowhelper++] = "";
                    }

                    if (dao.NeedDate != null)
                    {                       
                        if (dao.NeedDate.Year > 2000)
                        {
                            worksheet.Cells[i + 1, rowhelper++] = dao.NeedDate.ToString("yyyy-MMM-dd");
                        }
                        else
                        {
                            worksheet.Cells[i + 1, rowhelper++] = "";
                        }
                    }
                    else
                    {
                        worksheet.Cells[i + 1, rowhelper++] = "";
                    }


                   //date
                   // worksheet.Cells[i + 1, rowhelper++] = dao.TPCCRKDateLine;
                    if (dao.TPCCRKDateLine != null)
                    {
                        if (dao.TPCCRKDateLine.Year > 2000)
                        {
                            worksheet.Cells[i + 1, rowhelper++] = dao.TPCCRKDateLine.ToString("yyyy-MMM-dd");
                        }
                        else
                        {
                            worksheet.Cells[i + 1, rowhelper++] = "";
                        }
                    }
                    else
                    {
                        worksheet.Cells[i + 1, rowhelper++] = "";
                    }


                    worksheet.Cells[i + 1, rowhelper++] = dao.UNConfirmWC;
                    worksheet.Cells[i + 1, rowhelper++] = dao.ProdZEWO;
                    worksheet.Cells[i + 1, rowhelper++] = dao.RMTKPUR_POLN;//"RMl_TK_and_purch_part_po_ln";
                    worksheet.Cells[i + 1, rowhelper++] = dao.MaterialStatus;
                    worksheet.Cells[i + 1, rowhelper++] = dao.ProjectStatus;//OK

                    if (dao.ActCompltdDateFromSAP != null)
                    {                       
                        if (dao.ActCompltdDateFromSAP.Year > 2000)
                        {
                            worksheet.Cells[i + 1, rowhelper++] = dao.ActCompltdDateFromSAP.ToString("yyyy-MMM-dd");
                        }
                        else
                        {
                            worksheet.Cells[i + 1, rowhelper++] = "";
                        }
                    }
                    else
                    {
                        worksheet.Cells[i + 1, rowhelper++] = "";
                    }

                    
                   
                    worksheet.Cells[i + 1, rowhelper++] = dao.Price;

                    if (dao.ApproxComplDate != null)
                    {
                         
                            worksheet.Cells[i + 1, rowhelper++] = dao.ApproxComplDate;
                        
                    }
                    else
                    {
                        worksheet.Cells[i + 1, rowhelper++] = "";
                    }

                }//for


            }
            catch (Exception ee)
            {
                log.Info("Excel Final Error : " + ee.Message);
            }


            var backupFolder = ConfigurationManager.AppSettings["TechExcel_Folder"];
            String path = backupFolder + name + "-" + MYGlobal.getCurretYear() + ".xlsx";
            doDelete(path);
            Thread.Sleep(500);
          

                workbook.SaveAs(path, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                // Exit from the application  

                //MessageBox.Show("");

                app.Quit();



            
        }
         

    }
}
