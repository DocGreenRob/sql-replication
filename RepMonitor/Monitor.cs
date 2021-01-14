﻿using System;
using System.Configuration;
using System.IO;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Data;

namespace RepMonitor
{
    class Monitor
    {
        static void Main(string[] args)
        {
           try
            {

                string connectionString = ConfigurationManager.ConnectionStrings["Local"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_replmonitorhelpsubscription", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        for (int i = 0; i < args.Length; i++)
                        {
                            if (args[i] == "-publisher")
                            {
                                cmd.Parameters.Add("@publiser", SqlDbType.VarChar).Value = args[i + 1].ToString();
                            }
                            if (args[i] == "-publisher_db")
                            {
                                cmd.Parameters.Add("@publisher_db", SqlDbType.VarChar).Value = args[i + 1].ToString();
                            }
                            if (args[i] == "-publication")
                            {
                                cmd.Parameters.Add("@publication", SqlDbType.VarChar).Value = args[i + 1].ToString();
                            }
                            if (args[i] == "-publication_type")
                            {
                                cmd.Parameters.Add("@publication_type", SqlDbType.VarChar).Value = args[i + 1].ToString();
                            }
                            if (args[i] == "-mode")
                            {
                                cmd.Parameters.Add("@mode", SqlDbType.VarChar).Value = args[i + 1].ToString();
                            }
                            if (args[i] == "-topnum")
                            {
                                cmd.Parameters.Add("@topnum", SqlDbType.VarChar).Value = args[i + 1].ToString();
                            }
                            if (args[i] == "-exclude_anonymous")
                            {
                                cmd.Parameters.Add("@exclude_anonymous", SqlDbType.VarChar).Value = args[i + 1].ToString();
                            }
                            if (args[i] == "-refreshpolicy")
                            {
                                cmd.Parameters.Add("@refreshpolicy", SqlDbType.VarChar).Value = args[i + 1].ToString();
                            }
                        }
                        int ith = 1;
                        while (ith == 1)
                        {
                            Console.WriteLine(cmd.ExecuteReader());
                            //wait 5 min
                            System.Threading.Thread.Sleep(300000);
                        }

                        //Loop for 5 minutes
                    }
                }

            }
            catch (Exception ex)
            {
                Console.Write("Global Error: ");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
