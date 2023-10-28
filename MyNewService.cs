using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Timers;

namespace MyNewService
{
    public partial class MyNewService : ServiceBase
    {
        System.Timers.Timer timer = new System.Timers.Timer();
        string connectionString = "";

        public MyNewService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var time = DateTime.Now.ToLocalTime();
            WriteToFile("Service started at " + time);

            try
            {
                Assembly executingAssembly = Assembly.GetAssembly(typeof(ProjectInstaller));
                string targetDir = executingAssembly.Location;
                Configuration config = ConfigurationManager.OpenExeConfiguration(targetDir);
                long interval = Int32.Parse(config.AppSettings.Settings["interval"].Value.ToString());
                connectionString = config.AppSettings.Settings["connectionString"].Value.ToString();
                timer.Interval = interval;
                timer.Enabled = true;
                timer.Elapsed += (sender, eventArgs) => { OnTimeElapsed(); };
            }
            catch (Exception ex)
            {
                WriteToFile(ex.Message);
                throw new Exception(ex.Message);
            }

        }

        private void OnTimeElapsed()
        {
            var time = DateTime.Now.ToLocalTime();
            WriteToFile("Time is: " + time);

            string toBeWrittenInTheLog = "Accessing Database..." + "\n";

            SqlConnection connection = new SqlConnection(connectionString);
            try
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
            catch (SqlException e)
            {
                WriteToFile(e.Message);
            }
            finally
            {
                connection.Close();
            }
            //using (SqlConnection connection = new SqlConnection(connectionString))
            //{
            //    connection.Open();

            //toBeWrittenInTheLog += "Connection State: " + connection.State + "\n";
            //SqlCommand sqlCommand = new SqlCommand("select top 10 * from TutorialAppSchema.Users;", connection);

            //using (SqlDataReader reader = sqlCommand.ExecuteReader())
            //{
            //    while (reader.Read())
            //    {
            //        toBeWrittenInTheLog += reader["UserId"].ToString() + " " + reader["FirstName"].ToString() + " " + reader["LastName"].ToString() + " " + reader["Email"].ToString() + "\n";
            //    }
            //}
        
            toBeWrittenInTheLog += "Reading from Database ended..." + "\n";
            toBeWrittenInTheLog += "Exiting Database...";
            WriteToFile(toBeWrittenInTheLog);
        }

        protected override void OnStop()
        {
            var time = DateTime.Now.ToLocalTime();
            WriteToFile("Service stopped at: " + time);
        }

        public static void WriteToFile(string message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filepath = path + "\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                using (StreamWriter writer = File.CreateText(filepath))
                {
                    writer.WriteLine(message);
                }
            }
            else
            {
                using (StreamWriter writer = File.AppendText(filepath))
                {
                    writer.WriteLine(message);
                }
            }
        }
    }
}
