﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp
{
    class MYGlobal
    {
        public static string VERSION = "v-3.6.5";

        public static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static String PLANT_LION = "2088";
        public static String TEMPLOG_DB = ""; 
            public static String GAGETRACK_DB = "";

        public static String ACT_UPDATOR_LINE_TO_COMPLETE = "ACT_UPDATOR_LINE_TO_COMPLETE";
        public static String MSG_MASTER_LIST_UPDATE_STATUS = "update Material ";


        public static String TABLE_USER = "t_user";

        public static String PLANT_TIGER = "2750";

        public static String USE_DB = "";
        public static String USE_DB_NAME = "";
        public static String MEIE_USE_DB = "";
        public static String SYS_USE = "";

        public static int TEMP_ROLE_ADMIN = 1;
        public static int TEMP_ROLE_BUM = 2;
        public static int TEMP_ROLE_PGL = 3;
        public static int TEMP_ROLE_USER = 4;


        public static int ROLE_ADMIN_ID = 2;

        public static int ROLE_TPC_COORDINATOR_ID = 3;

        public static int ROLE_TPC_ROUTER_ID = 4;

        public static int ROLE_TPC_SOURCER_ID = 5;

        public static int ROLE_PC_PLANNER_ID = 7;

        public static int ROLE_TECH_MANAGER_ID = 9;

        public static int ROLE_TECH_ENGINEER_ID = 11;

        public static int ROLE_TPC_SUPPORT_ID = 12;

        public static int ROLE_TPC_PROGRAMMER_ID = 13;

        public static int ROLE_TPC_QC_ID = 14;

        public static int ROLE_TPC_WH_ID = 15;


        public static String HTML_BEGIN = "<html><body> ";
        public static String HTML_END = "</body> </html>";

        public static String htmlAlertBar()
        {
            return "<font size=\"2\" style=\"font-family:Calibri; \" >" +
                " <table border=\'0\' width=\'100%\' cellpadding=\"6\" cellspacing=\"2\"  style =\"color:#FFFFff\"> " +
                       " <tr ><td align=\'center\' bgcolor=\"#43494E\" style =\"color:#FFFFff\"> Generated by the system </td>  </tr>" +
                                                "</table></font >";
        }

        public static bool test = false;

        public static bool isTest()
        {
            return test;
           /* String test = MYGlobal.GetSettingValue("Test");
            if (test.Equals("True"))
            {
                return true;
            }
            else
            {
                return false;
            }*/
        }

        public static String getEmailFooter()
        {

            return "Thanks,  \n" + ", \nGenerated by the system!\n ";
        }

        public static String getEmailFooterHTML()
        {

            return "<font color =\'#43494E\' style=\"font-family:\'sans-serif\';\">  <P>Thanks,  <br>  </font>";
        }    

      /*  public static void print(String msg)
        {

           // System.Diagnostics.Debug.WriteLine(msg);
        }
*/

        public static string getToday()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        public static void sendHALEmail(String who, String subject, String to, String cc, String bcc, String message)
        {
            String now = DateTime.Now.ToString("MMM-dd HH:mm:ss");
            try
            {

                Microsoft.Office.Interop.Outlook.Application app = new Microsoft.Office.Interop.Outlook.Application();
                Microsoft.Office.Interop.Outlook.MailItem mailItem = app.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);

                mailItem.Subject = subject;
                mailItem.To = to;

                try
                {
                    if (cc != null && cc.Length > 0)
                    {
                        mailItem.CC = cc;
                    }
                }catch(Exception ee)
                {
                    LOG.Error("CC is null " + ee.Message);
                }

                try { 
                if (bcc != null && bcc.Length > 0)
                {
                    mailItem.BCC = bcc;
                }
                }
                catch (Exception ee)
                {
                    LOG.Error("BCC is null " + ee.Message);
                }

                mailItem.Importance = Microsoft.Office.Interop.Outlook.OlImportance.olImportanceHigh;

                LOG.Info(who + " , before body ....");
                //mailItem.Body = message + "\n\n\n" + getEmailFooter();
                String wish_user = "<p> <font color =\'#43494E\'  style=\"font-family:\'sans-serif\';\"> Dear " + who + ",</font></p> ";
                String add_msg = "<p> " + message + " </p>" + getEmailFooterHTML() + htmlAlertBar();//+ getEmailFooterHTML()


                String html_body = HTML_BEGIN + htmlAlertBar() + wish_user;
                html_body = html_body + add_msg;
                html_body = html_body + HTML_END;

                LOG.Info("html_body = " + html_body);

                //in-line image
                //AlternateView htmlView = AlternateView.CreateAlternateViewFromString(html_body, null, "text/html");

                

                mailItem.HTMLBody = html_body;

                mailItem.Importance = Microsoft.Office.Interop.Outlook.OlImportance.olImportanceHigh;//olImportanceLow

                mailItem.Display(false);

                ((Microsoft.Office.Interop.Outlook._MailItem)mailItem).Send();

                LOG.Info(who + " , EMail sent successfully");

            }
            catch (Exception ee)
            {
                LOG.Info("sendHALEmail Error [" + ee.Message + "], WHO=" + who + ", " + subject + ", to=" + to
                     + "," + "cc=" + cc + ", message=" + message);
                //MessageBox.Show("Error " + ee.Message);
            }
        }

        public static string GetSettingValue(string paramName)
        {
            return String.Format(ConfigurationManager.AppSettings[paramName]);
        }


        /***
         * public static String HALDB1_AUTH = "mssql";
	        public static String HALDB1_SERVER = "HCTNADBS001";
	        public static String HALDB1_DB = "CPS_DATA";
	        public static String HALDB1_USER = "TPC_APP_USER";
	        public static String HALDB1_PWD = "R4b*A6t#S8q~";
	
	        public static String HALDB2_AUTH = "mssql";
	        public static String HALDB2_SERVER = "AP1REPORTING";
	        public static String HALDB2_DB = "AP101_MPM_Dispatch";
	        public static String HALDB2_USER = "2088_REPORTS";
	        public static String HALDB2_PWD = "L4c^K0i~K1f#";	
        	public static String HALDB2A_DB = "AP101_MPM";
         * 
         */

        private static String CSTRING_HCTNADBS = @"Data Source=HCTNADBS001;Initial Catalog=CPS_DATA;User ID=TPC_APP_USER;Password=R4b*A6t#S8q~";
        public static String getCString4HCTNADBS()
        {
            return CSTRING_HCTNADBS;
        }

        private static String CSTRING_AP1REPORTING = @"Data Source=AP1REPORTING;Initial Catalog=AP101_MPM;User ID=2088_REPORTS;Password=L4c^K0i~K1f#";
        public static String getCString4AP1REPORTING()
        {
            return CSTRING_AP1REPORTING;
        }

       // No user account

        private static String CSTRING_SCGREPORTING = @"Data Source=SCGREPORTING;Initial Catalog=CAR01_CPS_RPTS;Integrated Security=True;";
        public static String getCString4SCGREPORTING()
        {
            return CSTRING_SCGREPORTING;
        }

        public static String getSing3HALRFQ()
        {
            return @"Data Source=DKTP611587\SQLEXPRESS;Initial Catalog=HalliburtonRFQ;User ID=sa;Password=ABCD7890&*();";
        }

        public static String getSing3HALMAINLotoLock()
        {
            return @"Data Source=DKTP611587\SQLEXPRESS;Initial Catalog=HAL_LOTOLOCK;User ID=sa;Password=ABCD7890&*();";
        }

        public static String getSing3MEIECString()
        {
            return @"Data Source=DKTP611587\SQLEXPRESS;Initial Catalog=SING3_MEIE;User ID=sa;Password=ABCD7890&*();";
        }

        public static String getCardevCString()
        {
            return @"Data Source=CARDEV001;Initial Catalog=TPC2021;User ID=CIMS_USER;Password=D2c?Z7w^E8e!;";
        }

        public static String getTechDBCString()
        {
            String techdb = MYGlobal.GetSettingValue("TECHDB_SERVER");
            String techdb_name = MYGlobal.GetSettingValue("TECHDB_NAME");

            if (techdb.Equals("Sing3"))
            {
                return @"Data Source=DKTP611587\SQLEXPRESS;Initial Catalog=" + techdb_name + ";User ID=sa;Password=ABCD7890&*();";
            }
            return null;
        }


        public static String getCString()
        {
            USE_DB = MYGlobal.GetSettingValue("DB");
            USE_DB_NAME = MYGlobal.GetSettingValue("DB_NAME");

            LOG.Info("use_db=" + MYGlobal.USE_DB);

            if (MYGlobal.USE_DB.Equals("APMFG"))
            {
                String uuu = @"Data Source=APMFGDBS001;Initial Catalog=" + USE_DB_NAME + ";User ID=CIMS_USER;Password=D2c?Z7w^E8e!;";
                return uuu;
            }

            else if (MYGlobal.USE_DB.Equals("ProDev"))
            {
                String uuu = @"Data Source=CARDEV001;Initial Catalog="+ USE_DB_NAME + ";User ID=CIMS_USER;Password=D2c?Z7w^E8e!;";
                return uuu;
              }
            else if (MYGlobal.USE_DB.Equals("Kams"))
            {
                return @"Data Source=NTBK616741\SQLEXPRESS;Initial Catalog=" + USE_DB_NAME + ";User ID=sa;Password=ABCD7890&*();";
            }
            else if (MYGlobal.USE_DB.Equals("Sing3"))
            {
                return @"Data Source=DKTP611587\SQLEXPRESS;Initial Catalog=" + USE_DB_NAME + ";User ID=sa;Password=ABCD7890&*();";
            }
            else if (MYGlobal.USE_DB.Equals("Sandbox"))
            {
                return @"Data Source=DKTP611587\SQLEXPRESS;Initial Catalog=" + USE_DB_NAME + ";User ID=sa;Password=ABCD7890&*();";
            }
            return null;
        }

        public static string getHCTDBCString()
        {
            return @"Data Source=HCTNADBS001;Initial Catalog=CPS_DATA;User ID=TPC_APP_USER;Password=R4b*A6t#S8q~";
        }

        public static string getAP1DBCStringx()
        {
            return @"Data Source=AP1REPORTING;Initial Catalog=AP101_MPM_Dispatch;User ID=2088_REPORTS;Password=L4c^K0i~K1f#";
        }

        public static string getAP1DBCStringAP1MPM()
        {
            return @"Data Source=AP1REPORTING;Initial Catalog=AP101_MPM;User ID=2088_REPORTS;Password=L4c^K0i~K1f#";
        }

        public static String getYesterday()
        {
          
            return DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        }

        public static String getCurretYear()
        {
            DateTime dateTime = DateTime.Today;
            return dateTime.ToString("yyyy");
        }

        public static String getCurretnDate()
        {
            DateTime dateTime = DateTime.Today;
            return dateTime.ToString("yyyy-MM-dd");
        }

        public static String getCurretnDateTime()
        {
            DateTime dateTime = DateTime.Today;
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static String getCurretnTime()
        {
            DateTime dateTime = DateTime.Now;
            return dateTime.ToString("HH:mm:ss");
        }
        //IYang30110249


        public static void sendHALSMTPEmail(String fromEmail, String who, String subject, String to, String cc, String bcc, String message)
        {

            var smtpClient = new SmtpClient("smtp.corp.halliburton.com")
            {
                Port = 25,
                Credentials = new NetworkCredential("kamesh.shankaran@halliburton.com", "ABC123"),
                EnableSsl = false,
            };


            String wish_user = "<p> <font color =\'#43494E\'  style=\"font-family:\'sans-serif\';\"> Dear " + who + ",</font></p> ";
            String add_msg = "<p> " + message + " </p>" + getEmailFooterHTML() + htmlAlertBar();//+ getEmailFooterHTML()
            String html_body = HTML_BEGIN + htmlAlertBar() + wish_user;
            html_body = html_body + add_msg;
            html_body = html_body + HTML_END;

            LOG.Info("html_body = " + html_body);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = html_body,// "<h1>Hello</h1>",
                IsBodyHtml = true,
            };
            ArrayList al = new ArrayList();
            //mailMessage.To.Add("kamesh.shankaran@halliburton.com");

            LOG.Info("to list " + to);
            //mailMessage.To.Add(to);


            try
            {
                if (to != null && to.Length > 0)
                {
                    string[] allcc = to.Split(';');
                    for (int x = 0; x < allcc.Length; x++)
                    {
                        string em = (string)allcc[x];
                        if (em.Length > 5)
                        {
                            mailMessage.To.Add(em);
                        }

                    }

                }
            }
            catch (Exception ee)
            {
                LOG.Error("To is null " + ee.Message);
            }


            try
            {
                if (cc != null && cc.Length > 0)
                {
                    string[] allcc = cc.Split(';');
                    for(int x = 0; x < allcc.Length; x++)
                    {
                        string em = (string)allcc[x];
                        if (em.Length > 5)
                        {
                            mailMessage.CC.Add(em);
                        }
                       
                    }
                  
                }
            }
            catch (Exception ee)
            {
                LOG.Error("CC is null " + ee.Message);
            }

            try
            {
                if (bcc != null && bcc.Length > 0)
                {
                    //mailMessage.Bcc.Add(bcc);
                    string[] allbcc = bcc.Split(';');
                    for (int x = 0; x < allbcc.Length; x++)
                    {
                        string em = (string)allbcc[x];
                        if (em.Length > 5)
                        {
                            mailMessage.Bcc.Add(em);
                        }

                    }

                }
            }
            catch (Exception ee)
            {
                LOG.Error("BCC is null " + ee.Message);
            }


            //final send email
            smtpClient.Send(mailMessage);

        }

         

        public static void DOClose(SqlConnection con, SqlDataAdapter adapter)
        {
            try
            {
                if(adapter!=null)
                    adapter.Dispose();
            }
            catch (Exception ex)
            {
                  LOG.Info("Error " + ex);
            }

            try
            {
                if (con != null)
                {
                    con.Close();
                }

            }
            catch (Exception ex)
            {
                LOG.Info("Error " + ex);
            }

        }

    }
}