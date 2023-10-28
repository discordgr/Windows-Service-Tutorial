using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MyNewService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            string connectionString = "";
            try
            {
                Assembly executingAssembly = Assembly.GetAssembly(typeof(ProjectInstaller));
                string targetDir = executingAssembly.Location;
                Configuration config = ConfigurationManager.OpenExeConfiguration(targetDir);
                connectionString = config.AppSettings.Settings["connectionString"].Value.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            var time = DateTime.Now.ToLocalTime();
            MyNewService.WriteToFile("Time is: " + time);

            string toBeWrittenInTheLog = "Accessing Database..." + "\n";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                toBeWrittenInTheLog += "Connection State: " + connection.State + "\n";
                SqlCommand sqlCommand = new SqlCommand("select top 10 * from TutorialAppSchema.Users;", connection);

                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        toBeWrittenInTheLog += reader["UserId"].ToString() + " " + reader["FirstName"].ToString() + " " + reader["LastName"].ToString() + " " + reader["Email"].ToString() + "\n";
                    }
                }

            }
            toBeWrittenInTheLog += "Reading from Database ended..." + "\n";
            toBeWrittenInTheLog += "Exiting Database...";
            MyNewService.WriteToFile(toBeWrittenInTheLog);

#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new MyNewService()
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
