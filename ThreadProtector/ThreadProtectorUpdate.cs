using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPC2UpdaterApp.DB;

namespace TPC2UpdaterApp.ThreadProtector
{
   public class ThreadProtectorUpdate
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void doThreadProtectorUpdate()
        {
            ArrayList al = getAllTPs();

            if (al.Count == 0)
            {
                //No need to send any email.
                return;
            }

            String html = "<h5> The following Thread Protector quantity are low ! </h5>  ";
            html = html + "<table width=\'80%\' align=\'center\' border=\'1\'> ";
            html = html + "<tr><td> Item# </td> <td> Item Desc </td> <td> Min Stock </td> <td> Reorder Lvl </td> <td> Reorder Qty </td> <td> Item Type </td> <td>" +
                " Item Size </td> <td> Current Qty </td> <td> Connection Type </td> <td> Material </td> <td> Vendor </td> </tr> ";


            for (int x = 0; x < al.Count; x++)
            {
                html = html + "<tr>";
                TPDao dao = (TPDao)al[x];

                html = html + "<td>"+dao.ItemNum +"</td>";               
                html = html + "<td>" + dao.ItemDesc + "</td>";
                html = html + "<td>" + dao.MinStock + "</td>";
                html = html + "<td>" + dao.ReOrderLevel + "</td>";
                html = html + "<td>" + dao.ReorderQty + "</td>";
                html = html + "<td>" + dao.ItemType + "</td>";
                html = html + "<td>" + dao.itemSize + "</td>";
                html = html + "<td>" + dao.CurrentQty + "</td>";
                html = html + "<td>" + dao.ConnectionType + "</td>";
                html = html + "<td>" + dao.Material + "</td>";
                html = html + "<td>" + dao.Vendor + "</td>";

                html = html + "</tr>";
            }

            html = html + "</table>";


            ArrayList alz = getAllActiveEmails();
            if (alz.Count == 0)
            {
                return;
            }
            else
            {
                string cc_emails = string.Empty;
                string to_email = string.Empty;
                for(int x = 0; x < alz.Count; x++)
                {
                    ThreadProtectorAlerEmailDao dao = (ThreadProtectorAlerEmailDao)alz[x];
                    if (x == 0)
                    {
                        to_email = dao.Email;
                    }
                    else
                    {
                        cc_emails = cc_emails + dao.Email + ";";
                    }
                   
                }


                MYGlobal.sendHALSMTPEmail("ThreadProtectorInventory@Halliburton.com", "All", "[Thread Protector Stock Update]", to_email, cc_emails, null, html);
            }

           

        }


        public static ArrayList getAllTPs()
        {
            ArrayList al = new ArrayList();
            string sql = "select item_no, item_desc,  min_stock, reorder_level, reorder_qty, item_type, item_size, current_qty, connection_type, material, vendor " +
                " from [t_thread_protr_item] WITH (NOLOCK)  where current_qty < reorder_level ";

            try
            {
                using (SqlConnection sqlCon = new SqlConnection(MYGlobal.getSing3MEIECString()))
                {
                    sqlCon.Open();
                    log.Info(" getAllTPs()  sql = " + sql);
                    using (SqlCommand cmd = new SqlCommand(sql, sqlCon))
                    {
                        cmd.CommandType = CommandType.Text;


                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                try
                                {
                                    TPDao dao = new TPDao();

                                    if ((reader["item_no"]) != DBNull.Value)
                                    {
                                        dao.ItemNum  = ((string)reader["item_no"]);
                                    }

                                    if ((reader["item_desc"]) != DBNull.Value)
                                    {
                                        dao.ItemDesc = ((string)reader["item_desc"]);
                                    }

                                    if ((reader["min_stock"]) != DBNull.Value)
                                    {
                                        dao.MinStock = ((int)reader["min_stock"]);
                                    }

                                    if ((reader["reorder_level"]) != DBNull.Value)
                                    {
                                        dao.ReOrderLevel = ((int)reader["reorder_level"]);
                                    }

                                    if ((reader["reorder_qty"]) != DBNull.Value)
                                    {
                                        dao.ReorderQty = ((int)reader["reorder_qty"]);
                                    }

                                    if ((reader["current_qty"]) != DBNull.Value)
                                    {
                                        dao.CurrentQty = ((int)reader["current_qty"]);
                                    }


                                    if ((reader["item_type"]) != DBNull.Value)
                                    {
                                        dao.ItemType = ((String)reader["item_type"]);
                                    }

                                    if ((reader["connection_type"]) != DBNull.Value)
                                    {
                                        dao.ConnectionType = ((String)reader["connection_type"]);
                                    }


                                    if ((reader["item_size"]) != DBNull.Value)
                                    {
                                        dao.itemSize = ((String)reader["item_size"]);
                                    }


                                    if ((reader["material"]) != DBNull.Value)
                                    {
                                        dao.Material = ((String)reader["material"]);
                                    }

                                    if ((reader["vendor"]) != DBNull.Value)
                                    {
                                        dao.Vendor = ((String)reader["vendor"]);
                                    }



                                    al.Add(dao);
                                }
                                catch (Exception ee)
                                {
                                    log.Error("Exception in getting   item_size = " + ee.Message);
                                    continue;
                                }


                            }
                        }


                        return al;
                    }
                }
            }
            catch (Exception ee)
            {
                log.Info("Error " + ee.Message);
            }

            return al;
        }



        public static ArrayList getAllActiveEmails()
        {
            ArrayList al = new ArrayList();
            string sql = "SELECT * from t_thread_protr_email_alert WITH (NOLOCK)  where status = 1";

            try
            {
                using (SqlConnection sqlCon = new SqlConnection(MYGlobal.getSing3MEIECString()))
                {
                    sqlCon.Open();
                    log.Info(" getAllActiveEmails()  sql = " + sql);
                    using (SqlCommand cmd = new SqlCommand(sql, sqlCon))
                    {
                        cmd.CommandType = CommandType.Text;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                try
                                {
                                    ThreadProtectorAlerEmailDao dao = new ThreadProtectorAlerEmailDao();

                                    if ((reader["name"]) != DBNull.Value)
                                    {
                                        dao.Name = ((string)reader["name"]);
                                    }

                                    if ((reader["email"]) != DBNull.Value)
                                    {
                                        dao.Email = ((string)reader["email"]);
                                    }

                                    if ((reader["status"]) != DBNull.Value)
                                    {
                                        dao.Status = ((int)reader["status"]);
                                    }
                                    
                                    al.Add(dao);
                                }
                                catch (Exception ee)
                                {
                                    log.Error("Exception in getting   item_size = " + ee.Message);
                                    continue;
                                }
                            }
                        }


                        return al;
                    }
                }
            }
            catch (Exception ee)
            {
                log.Info("Error " + ee.Message);
            }

            return al;
        }



    }
}
