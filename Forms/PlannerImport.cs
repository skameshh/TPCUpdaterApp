using ExcelDataReader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TPC2UpdaterApp.DB;
/**
 * 
 * \\AP1mfgfile\\sing3$\\Data\\Bulletin Board\\Production Inventory & Control\\Target List\\
 */
namespace TPC2UpdaterApp.Forms
{
    public partial class PlannerImport : Form
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public PlannerImport()
        {
            InitializeComponent();
            init();
        }

        // original file : Z:\Data\Bulletin Board\Production Inventory & Control\Target List\Target List\\TPC Assembly List.xlsx";

        //\\Ap1mfgfile\\SING3$\\Public\\

        static string dest_folder = "C:\\temp\\planner\\";
        //static String assembly_list_file_source_old = "\\\\AP1mfgfile\\sing3$\\Data\\Bulletin Board\\Production Inventory & Control\\Target List\\TPC Assembly List.xlsx";
        //\\ap1nas101it2944.file.core.windows.net\sing3\
        static String assembly_list_file_source = "\\\\ap1nas101it2944.file.core.windows.net\\sing3\\Data\\Bulletin Board\\Production Inventory & Control\\Target List\\TPC Assembly List.xlsx";

        static String assembly_list_file = dest_folder + "TPC Assembly List.xlsx";// "K:\\PROJECTS\\TPC-P2\\excels\\Assembly-test.xlsx";//test
        

        private void init()
        {
            timer1.Enabled = false;
            bool fe= fetchFromServer(assembly_list_file);
            log.Info("fe = " + fe +", file = "+ assembly_list_file);
            selectSheet();
            timer1.Enabled = true;

            //
            doCopyAndImport();
        }


        private void doCopyAndImport()
        {

            try
            {
                doCopy();

                doImportNew();
            }
            catch (Exception ee)
            {
                log.Info("Error in import " + ee.Message);
            }
        }


        private void doCopy()
        {
            //string sourceFile = @"C:\Temp\MaheshTX.txt";
            //string destinationFile = @"C:\Temp\Data\MaheshTXCopied.txt";

            string sourceFile = assembly_list_file_source;
            string destinationFile = assembly_list_file;
            try
            {
                if (File.Exists(sourceFile))
                {
                    log.Info("source file exists " + sourceFile);
                }
                else
                {
                    log.Error("Source file error = " + sourceFile);
                    return;
                }


                if (File.Exists(dest_folder))
                {
                    log.Info("source file exists " + dest_folder);

                }
                else
                {
                    System.IO.Directory.CreateDirectory(dest_folder);

                    log.Error("destination File   error = " + destinationFile);
                   // return;
                }



                File.Copy(sourceFile, destinationFile, true);
                log.Info("Import file success "+ destinationFile);
            }
            catch (IOException iox)
            {
               log.Error("Import file failed= " + iox.Message);
            }
        }


        DataTable dt;
        int empty_line = 0;
        DataTableCollection dataTableCollection;
        private bool fetchFromServer(String filePath)
        {
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = false }
                        });
                        dataTableCollection = result.Tables;
                        return true;
                        //cboSheet.Items.Clear();
                        //cboSheet.Items.AddRange(dataTableCollection.Cast<DataTable>().Select(t => t.TableName).ToArray<string>());
                    }
                }

            }
            catch (Exception ee)
            {
                log.Info("Error in fetching " + ee.Message);
                log.Error("Browse Error " + ee.Message);
               // MessageBox.Show("Browse Error " + ee.Message);
                doReset();
            }
            return false;
        }

        private void doReset()
        {
            empty_line = 0;
            dataTableCollection = null;
        }


        private void selectSheet()
        {
            try
            {
                if (dataTableCollection != null)
                {
                    log.Info("Selected sheet All");
                    // btnImportProject.Enabled = true;
                    dt = dataTableCollection["All"];//cboSheet.SelectedItem.ToString()
                    dataGridView1.DataSource = dt;
                }
                else
                {
                    log.Info("dataTableCollection IS NULL \n\n\n");
                }
                
            }
            catch (Exception ee)
            {
                log.Error("Error in select sheet : " + ee.Message);
            }
        }


        int pos_need_date = 0;
        int pos_promis_date = 1 ;
        
        int pos_pt_num = 3;
        int pos_matl_num = 7;
        int pos_prod_order = 11;
        int pos_unconfirm_wc = 12;       
        int pos_wc_delv_date = 13;
        int pos_wc_owner = 14;
        int pos_wc_remarks = 15;
        int pos_wc_vendor = 16;

        // December 12th remove DIQ by Elaine, which is after Vendor

        private void doImportNew()
        {
            Nullable<DateTime> null_date = null; ;
            log.Info("Planner doImport() started \n\n\n");

            //get no of rows
            int rowscount = dataGridView1.Rows.Count;
            if (rowscount > 25000)
            {
                rowscount = 20000;
            }

            try
            {

                for (int i = 0; i < rowscount; i++)//rowscount
                {
                    DateTime promised_date = null_date.GetValueOrDefault().Date;
                    DateTime need_date = null_date.GetValueOrDefault().Date;
                    DateTime wc_delv_date = null_date.GetValueOrDefault().Date;
                    String pt_num = "";
                    String matrl_num = "";
                    String prod_order = "";
                    String unconfirm_wc = "";
                    String wc_owner = "";
                    String wc_remarks = "";
                    String wc_vendor = "";


                    log.Info("\n\n\n ================== Doing Row " + i +"=============");
                    int cells_count = dataGridView1.Rows[i].Cells.Count;
                    log.Info("Cells count = " + cells_count);

                    if (cells_count > 1000)
                    {
                        MessageBox.Show("Too many cells, It may end up with memory error, Send Email ");
                        return;
                    }

                    try
                    {
                        need_date = dataGridView1.Rows[i].Cells[pos_need_date].Value == DBNull.Value ? null_date.Value : (DateTime)dataGridView1.Rows[i].Cells[pos_need_date].Value;
                        log.Info("Got need_date: " + need_date);
                    }
                    catch (Exception ee)
                    {
                        log.Info("Got need_date: error " + ee.Message);
                    }

                    try
                    {
                        promised_date = dataGridView1.Rows[i].Cells[pos_promis_date].Value == DBNull.Value ? null_date.Value : (DateTime)dataGridView1.Rows[i].Cells[pos_promis_date].Value;
                        log.Info("Got pos_promis_date from Excel: " + promised_date);
                    }
                    catch (Exception ee)
                    {
                        log.Info("Got promised_date: error " + ee.Message);
                    }

                    try
                    {
                        //
                        pt_num = dataGridView1.Rows[i].Cells[pos_pt_num].Value == DBNull.Value ? string.Empty : dataGridView1.Rows[i].Cells[pos_pt_num].Value.ToString(); ;
                        matrl_num = dataGridView1.Rows[i].Cells[pos_matl_num].Value == DBNull.Value ? string.Empty : dataGridView1.Rows[i].Cells[pos_matl_num].Value.ToString(); ;
                        prod_order = dataGridView1.Rows[i].Cells[pos_prod_order].Value == DBNull.Value ? string.Empty : dataGridView1.Rows[i].Cells[pos_prod_order].Value.ToString(); ;
                        unconfirm_wc = dataGridView1.Rows[i].Cells[pos_unconfirm_wc].Value == DBNull.Value ? string.Empty : dataGridView1.Rows[i].Cells[pos_unconfirm_wc].Value.ToString(); ;
                        wc_owner = dataGridView1.Rows[i].Cells[pos_wc_owner].Value == DBNull.Value ? string.Empty : dataGridView1.Rows[i].Cells[pos_wc_owner].Value.ToString(); ;
                        wc_remarks = dataGridView1.Rows[i].Cells[pos_wc_remarks].Value == DBNull.Value ? string.Empty : dataGridView1.Rows[i].Cells[pos_wc_remarks].Value.ToString(); ;
                        wc_vendor = dataGridView1.Rows[i].Cells[pos_wc_vendor].Value == DBNull.Value ? string.Empty : dataGridView1.Rows[i].Cells[pos_wc_vendor].Value.ToString(); ;
                    }catch(Exception ee)
                    {
                        log.Info("Got pt_num: " + pt_num +" ,ee="+ee.Message);
                        continue;
                    }
                    try
                    {
                        wc_delv_date = dataGridView1.Rows[i].Cells[pos_wc_delv_date].Value == DBNull.Value ? null_date.Value : (DateTime)dataGridView1.Rows[i].Cells[pos_wc_delv_date].Value;
                        log.Info("Got wc_delv_date: " + wc_delv_date);
                    }
                    catch (Exception ee)
                    {
                        log.Info("Got wc_delv_date: " + pt_num + " ,ee=" + ee.Message);
                    }

                    //Check wheter end reached
                    if (matrl_num.Equals(string.Empty))
                    {
                        if (empty_line <= 3)
                        {
                            empty_line++;
                            log.Info("End of the lines or reached empty line , empty_line = " + empty_line);
                            continue;
                        }
                        else
                        {
                            //MessageBox.Show("End of the lines or reached empty line, total materials : " + mat_list.Count);
                            log.Info("End of the lines or reached empty line");
                            break;
                        }

                    }


                    empty_line = 0;
                    T2MaterialDao mdao = new T2MaterialDao();
                    mdao.MaterialNbr = matrl_num;
                    mdao.ProdZEWO = prod_order ;
                    mdao.UNConfirmWC = unconfirm_wc;
                    mdao.WCDeliveryDate = wc_delv_date;
                    mdao.WCOwner = wc_owner;
                    mdao.WCRemarks = wc_remarks;
                    mdao.WCVendor = wc_vendor;

                    log.Info("From row = matrl_num=" + matrl_num + ",prod_order="+ prod_order + ", unconfirm_wc="+ unconfirm_wc);

                   /* DialogResult res = MessageBox.Show("Import", "import", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (res == DialogResult.No)
                    {
                        return;
                    }
*/


                    int project_id = 0;
                    int matl_id = 0;

                    //
                    T2ProjectDao pdao = new T2ProjectDao();
                    pdao.TrackingNumber = pt_num;
                    // ArrayList al = DBUtils.getAllT2Projects(pdao);
                    ArrayList al = DBUtils.getAllt2ProjectAndMaterailsForPlanner("2088", pt_num, matrl_num);

                    if (al.Count > 0)
                    {
                        T2ProjectMaterialDao redao = (T2ProjectMaterialDao)al[0];
                        project_id = redao.ProjectId;
                        matl_id = redao.MatlId;
                        if (project_id > 0)
                        {
                            //materaila must have id otherwise it will create new row
                            // mdao.Id = ;
                            mdao.ProjectId = project_id;
                            mdao.Id = matl_id;
                            try
                            {
                                string formatted = promised_date.ToString("yyyy-MM-dd");
                                mdao.WCPromisedDate = DateTime.Parse(formatted);
                            }catch(Exception ee)
                            {
                                log.Info("\n\n\n Date format error "+ promised_date + ee.Message + " \n\n\n");
                            }
                            int affrows = 0;
                            //==================
                            affrows =  DBUtils.doInsertUpdateT2Material4Planner(mdao);
                           log.Info("Now doing "+i +" ,   update material success , affrows= "+ affrows);
                            //=====================
                            //project promised date is updated from the Trigger.


                            log.Info("=========xxxxx== END OF Row " + i + "=========xxxxx \n\n\n ");
                        }
                    }
                    else
                    {
                        log.Info("Couldnt find project & Materials for  pt_num="+ pt_num + ", matrl_num="+ matrl_num +"\n\n\n");
                    }                   

                }//for


                log.Info("All rows done"+ rowscount + " \n\n");

            }//try
            catch(Exception ee)
            {
                log.Error("Error in importing " + ee.Message);
            }
        }




        private void dotest()
        {
            string pt_num = "PT07690";
            T2ProjectDao pdao = new T2ProjectDao();
            pdao.TrackingNumber = pt_num;
            ArrayList al = DBUtils.getAllt2ProjectAndMaterails("2088", "PT08408", "102891973");// DBUtils.getAllT2Projects(pdao);
            if (al.Count > 0)
            {
                T2ProjectMaterialDao redao = (T2ProjectMaterialDao)al[0];
                log.Info("Got Project " + redao.ProjectName +", "+redao.MaterialNbr);
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            doCopyAndImport();
            //dotest();
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            log.Info("Planner Timer tick \n\n\n");
            //Start only once.
            timer1.Enabled = false;
            doCopyAndImport();
            this.Visible = false;
        }
    }
}
