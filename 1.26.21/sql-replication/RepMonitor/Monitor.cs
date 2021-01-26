using System;
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
							conn.Open();
							using (SqlDataAdapter da = new SqlDataAdapter())
							{
								da.SelectCommand = cmd;
								using (DataTable dt = new DataTable())
								{
									da.Fill(dt);
									foreach (DataRow rw in dt.Rows)
									{
										Console.WriteLine("Status: " + GetStatus(rw[0].ToString()) 
															+ ", Warning: " + GetStatus(rw[1].ToString())
															+ ", Subscriber: " + rw[2].ToString()
															+ ", Subscriber db: " + rw[3].ToString());
									}
								}
							}
							conn.Close();
							//wait 5 seg
							System.Threading.Thread.Sleep(5000);
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

		private static string GetStatus(string statusId)
		{
			var status = int.Parse(statusId);
			switch (status)
			{
				case 0:
					return string.Empty;
				case 6:
					return "Failed";
				case 5:
					return "Retrying";
				case 4:
					return "Stopped";
				case 3:
					return "Idle";
				case 2:
					return "In progress";
				case 1:
					return "Started";
			}
			throw new Exception();
		}

		private string GetWarning(string warningId)
		{
			var warning = int.Parse(warningId);
			switch (warning)
			{
				case 0:
					return "-";
				case 1:
					return "expiration - a subscription to a transactional publication has not been synchronized within the retention period threshold.";
				case 2:
					return "latency - the time taken to replicate data from a transactional Publisher to the Subscriber exceeds the threshold, in seconds.";
				case 4:
					return "mergeexpiration - a subscription to a merge publication has not been synchronized within the retention period threshold.";
				case 8:
					return "mergefastrunduration - the time taken to complete synchronization of a merge subscription exceeds the threshold, in seconds, over a fast network connection.";
				case 16:
					return "mergeslowrunduration - the time taken to complete synchronization of a merge subscription exceeds the threshold, in seconds, over a slow or dial-up network connection.";
				case 32:
					return "mergefastrunspeed - the delivery rate for rows during synchronization of a merge subscription has failed to maintain the threshold rate, in rows per second, over a fast network connection.";
				case 64:
					return "mergeslowrunspeed - the delivery rate for rows during synchronization of a merge subscription has failed to maintain the threshold rate, in rows per second, over a slow or dial-up network connection.";
			}
			throw new Exception();
		}
	}
}
