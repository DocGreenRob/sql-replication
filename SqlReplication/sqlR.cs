using System;
using System.Configuration;
using System.IO;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

namespace SqlReplication
{
    class sqlR
    {
        static void Main(string[] args)
        {
            try
            {
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Local"].ConnectionString;

                if (args.Contains("-action"))
                {
                    //Distribution File
                    if (args[1].ToString().Equals("distribution"))
                    {
                        try
                        {
                            string pathDistributionFile = ConfigurationManager.AppSettings["DistributionFile"];

                            string script = File.ReadAllText(pathDistributionFile);

                            // split script on GO   command
                            System.Collections.Generic.IEnumerable<string> commandStrings = Regex.Split(script, @"^\s*GO\s*$",
                                                     RegexOptions.Multiline | RegexOptions.IgnoreCase);
                            using (SqlConnection connection = new SqlConnection(connectionString))
                            {
                                connection.Open();
                                foreach (string commandString in commandStrings)
                                {
                                    if (commandString.Trim() != "")
                                    {
                                        using (var command = new SqlCommand(commandString, connection))
                                        {
                                            try
                                            {
                                                command.ExecuteNonQuery();
                                            }
                                            catch (SqlException ex)
                                            {
                                                string spError = commandString.Length > 100 ? commandString.Substring(0, 100) + " ...\n..." : commandString;
                                                Console.WriteLine(string.Format("Please check the SqlServer script.\nFile: {0} \nLine: {1} \nError: {2} \nSQL Command: \n{3}", pathDistributionFile, ex.LineNumber, ex.Message, spError));

                                            }
                                        }
                                    }
                                }
                                connection.Close();
                                Console.WriteLine("Distribution created successfully.");
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    //Publication File
                    else if (args[1].ToString().Equals("publication"))
                    {
                        try
                        {
                            string pathDistributionFile = ConfigurationManager.AppSettings["PublicationFile"];

                            string script = File.ReadAllText(pathDistributionFile);

                            //Parse Vales
                            bool db = false, pn = false, tn = false, lu = false, lp = false;
                            for (int i = 0; i < args.Length; i++)
                            {
                                if (args[i] == "-database")
                                {
                                    script = script.Replace("{{database_name}}", args[i + 1].ToString());
                                    db = true;
                                }
                                if (args[i] == "-replpublicationname")
                                {
                                    script = script.Replace("{{publication_name}}", args[i + 1].ToString());
                                    pn = true;
                                }
                                if (args[i] == "-tablenames")
                                {
                                    script = script.Replace("{{tables_names}}", args[i + 1].ToString());
                                    tn = true;
                                }
                                if (args[i] == "-sqlloginuser")
                                {
                                    script = script.Replace("{{sql_user}}", args[i + 1].ToString());
                                    lu = true;
                                }
                                if (args[i] == "-sqlloginpassword")
                                {
                                    script = script.Replace("{{user_pwd}}", args[i + 1].ToString());
                                    lp = true;
                                }
                            }

                            if (!db || !pn || !tn || !lu || lp)
                            {
                                if (!db)
                                {
                                    Console.WriteLine("Please add the -database argument...");
                                }
                                if (!pn)
                                {
                                    Console.WriteLine("Please add the -replpublicationname argument...");
                                }
                                if (!tn)
                                {
                                    Console.WriteLine("Please add the -tablenames argument...");
                                }
                                if (!lu)
                                {
                                    Console.WriteLine("Please add the -sqlloginuser argument...");
                                }
                                if (!lp)
                                {
                                    Console.WriteLine("Please add the -sqlloginpassword argument...");
                                }
                                Console.WriteLine("Publication failed...");
                            }
                            else
                            {
                                // split script on GO command
                                System.Collections.Generic.IEnumerable<string> commandStrings = Regex.Split(script, @"^\s*GO\s*$",
                                                         RegexOptions.Multiline | RegexOptions.IgnoreCase);
                                using (SqlConnection connection = new SqlConnection(connectionString))
                                {
                                    connection.Open();
                                    foreach (string commandString in commandStrings)
                                    {
                                        if (commandString.Trim() != "")
                                        {
                                            using (var command = new SqlCommand(commandString, connection))
                                            {
                                                try
                                                {
                                                    command.ExecuteNonQuery();
                                                }
                                                catch (SqlException ex)
                                                {
                                                    string spError = commandString.Length > 100 ? commandString.Substring(0, 100) + " ...\n..." : commandString;
                                                    Console.WriteLine(string.Format("Please check the SqlServer script.\nFile: {0} \nLine: {1} \nError: {2} \nSQL Command: \n{3}", pathDistributionFile, ex.LineNumber, ex.Message, spError));

                                                }
                                            }
                                        }
                                    }
                                    connection.Close();
                                    Console.WriteLine("Publication created successfully.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    //Subscription File
                    else if (args[1].ToString().Equals("subscription"))
                    {
                        try
                        {
                            string pathDistributionFile = ConfigurationManager.AppSettings["SubscriptionFile"];

                            string script = File.ReadAllText(pathDistributionFile);

                            bool db = false, pn = false, si = false, sdb = false, lu = false, lp = false;
                            //Parse Vales
                            for (int i = 0; i < args.Length; i++)
                            {
                                if (args[i] == "-database")
                                {
                                    script = script.Replace("{{database_name}}", args[i + 1].ToString());
                                    db = true;
                                }
                                if (args[i] == "-replpublicationname")
                                {
                                    script = script.Replace("{{publication_name}}", args[i + 1].ToString());
                                    pn = true;
                                }
                                if (args[i] == "-subscriber_ip")
                                {
                                    script = script.Replace("{{subscriber_ip}}", args[i + 1].ToString());
                                    si = true;
                                }
                                if (args[i] == "-subscriberDB")
                                {
                                    script = script.Replace("{{sub_db}}", args[i + 1].ToString());
                                    sdb = true;
                                }
                                if (args[i] == "-sqlloginuser")
                                {
                                    script = script.Replace("{{sql_user}}", args[i + 1].ToString());
                                    lu = true;
                                }
                                if (args[i] == "-sqlloginpassword")
                                {
                                    script = script.Replace("{{user_pwd}}", args[i + 1].ToString());
                                    lp = true;
                                }
                            }


                            if (!db || !pn || !si || !sdb || !lu || lp)
                            {
                                if (!db)
                                {
                                    Console.WriteLine("Please add the -database argument...");
                                }
                                if (!pn)
                                {
                                    Console.WriteLine("Please add the -replpublicationname argument...");
                                }
                                if (!si)
                                {
                                    Console.WriteLine("Please add the -subscriber_ip argument...");
                                }
                                if (!sdb)
                                {
                                    Console.WriteLine("Please add the -subscriberDB argument...");
                                }
                                if (!lu)
                                {
                                    Console.WriteLine("Please add the -sqlloginuser argument...");
                                }
                                if (!lp)
                                {
                                    Console.WriteLine("Please add the -sqlloginpassword argument...");
                                }
                                Console.WriteLine("Subscription failed...");
                            }
                            else
                            {

                                // split script on GO command
                                System.Collections.Generic.IEnumerable<string> commandStrings = Regex.Split(script, @"^\s*GO\s*$",
                                                     RegexOptions.Multiline | RegexOptions.IgnoreCase);
                                using (SqlConnection connection = new SqlConnection(connectionString))
                                {
                                    connection.Open();
                                    foreach (string commandString in commandStrings)
                                    {
                                        if (commandString.Trim() != "")
                                        {
                                            using (var command = new SqlCommand(commandString, connection))
                                            {
                                                try
                                                {
                                                    command.ExecuteNonQuery();
                                                }
                                                catch (SqlException ex)
                                                {
                                                    string spError = commandString.Length > 100 ? commandString.Substring(0, 100) + " ...\n..." : commandString;
                                                    Console.WriteLine(string.Format("Please check the SqlServer script.\nFile: {0} \nLine: {1} \nError: {2} \nSQL Command: \n{3}", pathDistributionFile, ex.LineNumber, ex.Message, spError));

                                                }
                                            }
                                        }
                                    }
                                    connection.Close();
                                    Console.WriteLine("Subscription created successfully.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No action argument provided.");
                    Console.WriteLine("Action argument needs to be the first one in the command line.");
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
