using Quartz;
using Quartz.Impl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TPC2UpdaterApp.DB;
using TPC2UpdaterApp.Forms;
using TPC2UpdaterApp.Helpers;
using TPC2UpdaterApp.Jobs;
using TPC2UpdaterApp.ProcurementDIQ;
using TPC2UpdaterApp.TestCellBackupReport;
using TPC2UpdaterApp.ThreadProtector;
using TPC2UpdaterApp.Trigger;
using TPC2UpdaterApp.warehouse;

/**
 * 
 *  ------Make Parts-------
 * select  project_id, material_num, material_status, routing_scope, prod_ZEWO ,act_compltd_date_from_SAP 
from t2_material 
where act_compltd_date_from_SAP is null
and routing_scope  in ('Assembly', 'Farm-Out', 'In-House', 'Turn Key'); --1729

--AP1Reporting-----
select  MTRL_OID, PROD_ORDR_NBR, TOT_ORDR_QTY, ACT_CMPLT_DT ,  PROD_ORDR_DSC 
             from PROD_ORDR where PROD_ORDR_NBR like '%106725870%';


-- ------System PO Tagged - GI date-------------
select project_id, id as matlId, material_num, material_status, routing_scope, prod_ZEWO, raw_matl_assignment, RM_TK_Purch_GR , act_compltd_date_from_SAP
			from t2_material where raw_matl_assignment='Tagged'
			and act_compltd_date_from_SAP is null;
-- HCT----------
select h.Material_Doc, h.Transaction_Date_Only, h.Material_NOZERO, h.Wip_Order, h.Wip_Order_Type 
                from MATERIAL_ISSUE_HISTORY h  where h.Wip_order like '%106799483%' and Material_NOZERO='101710555';


 ---------Manual PO----------
 select project_id, id as matlId,  material_num, material_status, routing_scope, prod_ZEWO, raw_matl_assignment, RM_TK_Purch_GR , act_compltd_date_from_SAP 
			from t2_material where act_compltd_date_from_SAP is null
			and routing_scope in ('Purchase', 'From Stock' )  and raw_matl_assignment not in ('Tagged')
			and material_status='Completed';
-- Just copy GR Date to act_compltd_date_from_SAP column
 * 
 * 
 * 
 */
namespace TPC2UpdaterApp
{
    public partial class Form1 : Form
    {
        private bool IS_TEST = false;//coming from app.config

        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Form1()
        {
            InitializeComponent();
            init();
        }

        private void init()
        {

           String test_cal =  MYGlobal.GetSettingValue("IS_Test");
            this.Text = "TPC2UpdatorApp " + MYGlobal.VERSION + " | "+getToday();

            if (test_cal.Equals("True"))
            {
                IS_TEST = true;
            }
            else
            {
                IS_TEST = false;
            }

            if (IS_TEST)
            {
                btnStart.Text = "TEST";
            }
            else
            {
                btnStart.Text = "Production";
            }


            lblDBSettings.Text = MYGlobal.GetSettingValue("DB") + " : " + MYGlobal.GetSettingValue("DB_NAME");
            SqlConnection con= DBUtils.getSQLConnection();
            if (con != null)
            {               
                MessageBox.Show("DB Connection success");
                con.Close();
                con = null;
            }
            else
            {
                MessageBox.Show("DB Connection Failed");
                Environment.Exit(0);
            }
        }

        private string getToday()
        {
            return DateTime.Now.ToString("yyyy-MMM-dd");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
           DialogResult res =  MessageBox.Show("Are you sure to close", "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (res == DialogResult.Yes) {
                Environment.Exit(0);
            }
            else
            {
                this.Parent = null;
                e.Cancel = true;
                return;
            }
           
        }


        /**
        static String t1 = "0 15 7 * * ?";
        static String t_every_10_minutes = "0 0/10 * * * ?";
        static String t_12_05_pm = "0 5 12 * * ?";
        static String t_7_15_am = "0 15 7 * * ?";
        static String t_11_05_am = "0 5 11 * * ?";
         * 
         */
        private async void doTask()
        {
            log.Info("doTask() started ");
            try
            {
                // construct a scheduler factory
                ISchedulerFactory schedFact = new StdSchedulerFactory();

                // get a scheduler
                IScheduler sched = await schedFact.GetScheduler();
                await sched.Start();

                //========  SAMPLE  ===================
                IJobDetail job = JobBuilder.Create<MYJob>()
                    .WithIdentity("myJob", "group1")
                    .Build();

                //cronExpression: "0/5 * * * * ?")); 
                ITrigger pglTeamAlertYesterdayTrigger = TriggerBuilder.Create()
                   .WithCronSchedule("0 3,55 20,22 * * ?")
                   .Build();

                //add to scheduler
                await sched.ScheduleJob(job, pglTeamAlertYesterdayTrigger);

                //=======================================


                //========  ME2N Job & Trigger  ===================
                //========  FO Job & Trigger  ===================
                //========  UNSCH Job & Trigger  ===================
                //========  ZM33 Job & Trigger  ===================
                IJobDetail me2njob = JobBuilder.Create<ME2NJob>()
                    .WithIdentity("me2njob", "group1")
                    .Build();

                //cronExpression: "0/5 * * * * ?")); 
                ITrigger me2nTrigger = TriggerBuilder.Create()
                    //.WithCronSchedule("0 0 0/1 * * ?")
                    .WithCronSchedule("0 0 5,11,20 * * ?")
                   .Build();

                if (!IS_TEST)
                {
                    //add to scheduler
                    await sched.ScheduleJob(me2njob, me2nTrigger);
                }
                //=======================================                  


                //========  Planner Job & Trigger 10.30am =================== //
                //========== Also have ThreadProtector  update ============== //
                IJobDetail plannerjob_10_30 = JobBuilder.Create<PlannerJob>()
                    .WithIdentity("plannerjob_10_30", "group1")
                    .Build();

                //cronExpression: "0/5 * * * * ?")); 
                ITrigger plannerTrigger_10_30 = TriggerBuilder.Create()
                   .WithCronSchedule("0 30 4,10,12,18 * * ?")
                   .Build();

                if (!IS_TEST)
                {
                    //add to scheduler
                    await sched.ScheduleJob(plannerjob_10_30, plannerTrigger_10_30);
                }

    

                //========  Backup Job & Trigger 6.30am ===================
                IJobDetail backup_job = JobBuilder.Create<BackupDBJob>()
                    .WithIdentity("backup_job", "group1")
                    .Build();

                //cronExpression: "0/5 * * * * ?")); 
                ITrigger backup_trigger = TriggerBuilder.Create()
                   .WithCronSchedule("0 50 23 * * ?")
                   .Build();

                if (!IS_TEST)
                {
                    //add to scheduler
                    await sched.ScheduleJob(backup_job, backup_trigger);
                }
                    

            
                //========  GlobalReport Job & Trigger 21.40PM ===================
                IJobDetail global_report_job_11_30 = JobBuilder.Create<GlobalReportJob>()
                    .WithIdentity("global_report_job_11_30", "group1")
                    .Build();

                //cronExpression: "0/5 * * * * ?")); 
                ITrigger global_report_trigger_11_30 = TriggerBuilder.Create()
                   .WithCronSchedule("0 40 4,13,21 * * ?")
                   .Build();

                if (!IS_TEST)
                {
                    //add to scheduler
                    await sched.ScheduleJob(global_report_job_11_30, global_report_trigger_11_30);
                }


                //========  Tech Excel Job & Trigger 21.40PM ===================
                IJobDetail techexcel_report_job_5am_ = JobBuilder.Create<TechExcelJob>()
                    .WithIdentity("techexcel_report_job_5am_", "group1")
                    .Build();

                //cronExpression: "0/5 * * * * ?")); 
                ITrigger techexcel_report_trigger_5am_ = TriggerBuilder.Create()
                   .WithCronSchedule("0 30 5 * * ?")
                   .Build();

                if (!IS_TEST)
                {
                    //add to scheduler
                    await sched.ScheduleJob(techexcel_report_job_5am_, techexcel_report_trigger_5am_);
                }




                //========  QC Operations Job & Trigger 5am ===================
                IJobDetail qc_operation_job = JobBuilder.Create<QCJob>()
                    .WithIdentity("qc_operation_job", "group1")
                    .Build();

                //cronExpression: "0/5 * * * * ?")); 
                ITrigger qc_operation_trigger = TriggerBuilder.Create()
                   .WithCronSchedule("0 05 5,22 * * ?")
                   .Build();

                if (!IS_TEST)
                {
                    //add to scheduler
                    await sched.ScheduleJob(qc_operation_job, qc_operation_trigger);
                }

 

                //======== Testcell backup 11.45pm ===================
                // disabled 
               /* IJobDetail testcell_bup_job = JobBuilder.Create<TestCellBackupJob>()
                    .WithIdentity("testcell_bup_job", "group1")
                    .Build();

                //cronExpression: "0/5 * * * * ?")); 
                ITrigger testcell_bup_trigger = TriggerBuilder.Create()
                   .WithCronSchedule("0 45 23 * * ?")
                   .Build();

                if (!IS_TEST)
                {
                    //add to scheduler
                    await sched.ScheduleJob(testcell_bup_job, testcell_bup_trigger);
                }*/



                //========  Procurement report 4pm | 16.02 ===================
                //======== sending email ===============
                IJobDetail proc_diq_job = JobBuilder.Create<ProcDiqJob>()
                    .WithIdentity("proc_diq_job", "group1")
                    .Build();

                //cronExpression: "0/5 * * * * ?")); 
                ITrigger proc_diq_trigger = TriggerBuilder.Create()
                   .WithCronSchedule("0 02 16 * * ?")
                   .Build();

                if (!IS_TEST)
                {
                    //add to scheduler
                    await sched.ScheduleJob(proc_diq_job, proc_diq_trigger);
                }



                //========  Every one hour master routing ===================
                /*IJobDetail master_routing_job = JobBuilder.Create<MasterRoutingJob>()
                    .WithIdentity("master_routing_job", "group1")
                    .Build();

                //cronExpression: "0/5 * * * * ?")); 
                ITrigger master_routing_trigger = TriggerBuilder.Create()
                   .WithCronSchedule("0 * * * * ?")
                   .Build();

                if (!IS_TEST)
                {
                    //add to scheduler
                    await sched.ScheduleJob(master_routing_job, master_routing_trigger);
                }*/

                //========  QC Report Yesterday afternoon done ===================
                IJobDetail qc_report_am_job = JobBuilder.Create<QCReportJob>()
                    .WithIdentity("qc_report_am_job", "group1")
                    .Build();

                //cronExpression: "0/5 * * * * ?")); 
                ITrigger qc_report_am_trigger = TriggerBuilder.Create()
                   .WithCronSchedule("0 30 08 * * ?")
                   .Build();

                if (!IS_TEST)
                {
                    //add to scheduler
                    await sched.ScheduleJob(qc_report_am_job, qc_report_am_trigger);
                }

                //========  QC Report Today mornign done ===================
                IJobDetail qc_report_pm_job = JobBuilder.Create<QCReportPMJob>()
                    .WithIdentity("qc_report_pm_job", "group1")
                    .Build();

                //cronExpression: "0/5 * * * * ?")); 
                ITrigger qc_report_pm_trigger = TriggerBuilder.Create()
                   .WithCronSchedule("0 30 13 * * ?")
                   .Build();

                if (!IS_TEST)
                {
                    //add to scheduler
                    await sched.ScheduleJob(qc_report_pm_job, qc_report_pm_trigger);
                }


                log.Info("\n\n All tasks started successfully \n\n");

            }
            catch (Exception ee)
            {
                log.Error("\n\n==============================\n\n");
   
                   log.Error("\n\n Error in loading tasks doTask() " + ee.Message +"\n\n");
                log.Error("\n\n==============================\n\n");
            }
        }



        //==================================================================








        private void btnStart_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("You pressed it");
            doTask();
            btnStart.BackColor = Color.DarkGreen;
            btnStart.Enabled = false;
        }

        private void btnTestFO_Click(object sender, EventArgs e)
        {
            FOTrigger.doFOUpdate();
        }

        private void btnRmtk_Click(object sender, EventArgs e)
        {
            ME2NTrigger.doME2NUpdate();
        }

        private void btnUnSch_Click(object sender, EventArgs e)
        {
            UNschTrigger.doUpdateUnSch();
        }

        private void btnZm33_Click(object sender, EventArgs e)
        {
            ZM33Trigger.doUpdateZM33();
        }


        private void doReadPlannerExcel()
        {

        }


        private void btnPlannerExcel_Click(object sender, EventArgs e)
        {
            PlannerImport pi = new PlannerImport();
            pi.Show();
        }

       
        private void btnBackupDB_Click(object sender, EventArgs e)
        {
            if (IS_TEST)
            {
                // BackupDBHelper.doBackup();
               // string rfq_backupFolder = ConfigurationManager.AppSettings["BackupFolder_RFQ"];
               // BackupDBHelper.doBackup(rfq_backupFolder, MYGlobal.getSing3HALRFQ());
                MessageBox.Show("backup done");
            }
            else
            {
                MessageBox.Show ("Sorry running production");
            }
              
        }


        private void btnGlobalReport_Click(object sender, EventArgs e)
        {
            if (IS_TEST)
            {
                GlobalReportHelper.doGlobalReport();
            }
            else
            {
                MessageBox.Show("Sorry running production");
            }
            // getAllMakePartsActCompletionDate("106771172");
        }

        private void btnQCTest_Click(object sender, EventArgs e)
        {
            QCHelper.doQCUpdate();
        }

        private void btnZewoStatus_Click(object sender, EventArgs e)
        {
           // ZEWOStatusUpdate4Planner.doUpdateZewostatus();
        }

      
        private void btnTest_Click(object sender, EventArgs e)
        {
            btnTest.Enabled = false;
            //String ss = "000106858886";
            // log.Info("After : "+ss.Substring(3));
            //MYGlobal.sendHALSMTPEmail("Kamesh", "Test SMTP", "kamesh.shankaran@halliburton.com", "skameshh@gmail.com;skameshh@outlook.com", null, "<h1> Welcome </h1> ");

            //this will send email if the cound is low
            // ThreadProtectorUpdate.doThreadProtectorUpdate();

            //added at  GlobalReportHelper.
            //VendorUpdator.doFetchFromMMAndUpdateMaterial();

            //TechInventoryDBUpdator.doTechInventory();

            //  PlannerAppoxiEstmateDate.doPurchaseUpdate();//.doUpdateApproxPromixedDate();

            // DriftGageTrackSync.doAllGageTrackUpdate();

            // TecellReportHelper.doSendBackupEMail();

            // AnTFixtureSearchInspectionHistoryHelper. doSendFixtureInspEMail();

            //  TPCPPPriceUpdateHelper.doUpdatePurchasePartsPrice();

            //Master Routing
            //   TPCMasterRouting.doMasterRouting();



            //TPCMakePartsriceUpdateHelper.doUpdateMakePartsPrice();

            //  

            //PlannerTrigger.doPlanner();


            //OK  20th Sep
            // TurnkeyFarmoutHelper.doUpdateFarmoutTurnkey();
            //TPCMasterRouting.doMasterRouting();
            //TPCMakePartsriceUpdateHelper.doUpdateMakePartsPrice();

            //GlobalReportHelper.doUpdateGlobalReport();


            /// = ====1 Procurement page , missing P-Info, form-out items missing
            //   TurnkeyFarmoutHelper.doUpdateFarmoutTurnkey();
            /// = 2
            //  ProcurementDIQUpdator.doProcrUpdate(); //email

            //This updates PO = RMl_TK_and_purch_part_po_ln
            //ProcurementPOPRUpdate.doUpdate();

            //This updates PREQ = RMl_TK_and_purch_part_Preq
            //  ProcurementTKFOPRUpdate.doUpdate();
            // doTest();

            //today
            //DateTime dt1 = DateTime.Now;
            //  Custom
            String strdt1 = "2023-05-12";
            DateTime dt1 = DateTime.Parse(strdt1);
            WHUpdator.doWarehouseQC(dt1, "PM");


            //== no use.. just change date above ===
            //WHUpdator.doEmptyEmail(dt1, "PM");
            //yesterday
            //  DateTime dt2 =  dt1.AddDays(-1);d"));
            //  Custom
            //  log.Info("Date yesterday " + dt2.ToString("yyyy-MM-d


            //  WHUpdator.doWarehouseQC(dt2, "AM");
            //=======temporary===========
            //   WHUpdator.doTempUpdate();
            //===============

            //Planner update Dont use
            // //ZEWOStatusUpdate4Planner.doUpdateZewostat() //dont use this
            //============================

             

        }


        private static string NCR = "Cr ";
        private void doTest()
        {
            log.Info("len = " + NCR.Length);
            if (NCR.Length > 3)
            {
              //  NCR = NCR.Substring(0, 3);
            }
            log.Info("NCR = " +NCR +", Len=" + NCR.Length);

            string critical_or_non = NCR;

            if (critical_or_non.Contains("NCr"))
            {
                critical_or_non = "NCr";
            }
            else
            {
                critical_or_non = "Cr";
            }

            log.Info("NCR = " + critical_or_non + ", Len=" + critical_or_non.Length);
        }


        private void btnTechInventory_Click(object sender, EventArgs e)
        {
            TechInventoryDBUpdator.doTechInventory();
        }

        private void btnProcurementDIQ_Click(object sender, EventArgs e)
        {
           DialogResult res =  MessageBox.Show("Send Procurement DIQ Email?", "Procurement", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (res == DialogResult.Yes)
            {
                ProcurementDIQUpdator.doProcrUpdate();
            }
        }

        private void btnProcurement_Click_1(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("Update Procurement ?", "Procurement", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (res == DialogResult.Yes)
            {
                TurnkeyFarmoutHelper.doUpdateFarmoutTurnkey();
            }
        }
        private void doMasterRouting()
        {
            try
            {
                Thread.Sleep(1000);
                TPCMasterRouting.doMasterRouting();

            }
            catch (Exception ee)
            {
                log.Info("EE = " + ee.Message);
            }
        }

        private void btnMasterRouting_Click(object sender, EventArgs e)
        {
            doMasterRouting();
        }

        private void btnTechMgrExcel_Click(object sender, EventArgs e)
        {
            TechManagerExcel.doTechMgrExcel();
        }
    }
}
