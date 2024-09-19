using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Timers;

namespace FileTransfer_WindowService
{
    public partial class Service1 : ServiceBase
    {
        private Timer timer = new Timer(); // Timer to poll the directory
        private readonly string sourceFolder = @"D:\Textfile"; // Path to monitor
        private readonly string processedFolder = @"D:\ProcessedFile"; // Folder to move processed files
        private readonly string connectionString = @"server=NEXSUS-DV94\SQLEXPRESS;Initial Catalog=WindowService;User Id=sa;Password=ccntspl@123;TrustServerCertificate=True";

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("Service started at " + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 30000; // 30 seconds
            timer.Enabled = true;
        }

        protected override void OnStop()
        {
            WriteToFile("Service stopped at " + DateTime.Now);
            timer.Enabled = false;
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            WriteToFile("Polling the directory at " + DateTime.Now);
            ProcessFilesInDirectory();
        }

        private void ProcessFilesInDirectory()
        {
            try
            {
                if (!Directory.Exists(sourceFolder))
                {
                    WriteToFile($"Source folder does not exist: {sourceFolder}");
                    return;
                }

                string[] files = Directory.GetFiles(sourceFolder, "*.txt");
                foreach (string filePath in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    string[] fileNameParts = fileName.Split(' ');

                    if (fileNameParts.Length >= 3)
                    {
                        string channel = fileNameParts[0].Substring(1, 1); // e.g., P0 -> channel = 0
                        string date = fileNameParts[1]; // e.g., 01-10-2011
                        string time = fileNameParts[2]; // e.g., 14:25:36

                        // Convert date to proper format
                        DateTime parsedDate;
                        if (DateTime.TryParseExact(date, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out parsedDate))
                        {
                            // Save details to database
                            SaveToDatabase(channel, parsedDate, time);

                            // Move the processed file to the processed folder
                            if (!Directory.Exists(processedFolder))
                            {
                                Directory.CreateDirectory(processedFolder);
                            }
                            string destFilePath = Path.Combine(processedFolder, Path.GetFileName(filePath));
                            File.Move(filePath, destFilePath);

                            WriteToFile($"File {filePath} processed and moved to {destFilePath} at {DateTime.Now}");
                        }
                        else
                        {
                            WriteToFile($"Invalid date format in file name: {filePath}");
                        }
                    }
                    else
                    {
                        WriteToFile($"Invalid file name format: {filePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToFile($"Error during file processing: {ex.Message}");
            }
        }

        private void SaveToDatabase(string channel, DateTime date, string time)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO FileDetails (Channel, Date, Time) VALUES (@Channel, @Date, @Time)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Channel", channel);
                        cmd.Parameters.AddWithValue("@Date", date);
                        cmd.Parameters.AddWithValue("@Time", time);
                        cmd.ExecuteNonQuery();
                    }
                }
                WriteToFile($"Data saved to database: Channel={channel}, Date={date.ToShortDateString()}, Time={time}");
            }
            catch (Exception ex)
            {
                WriteToFile($"Error saving to database: {ex.Message}");
            }
        }

        private void WriteToFile(string message)
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string filePath = Path.Combine(path, $"ServiceLog_{DateTime.Now:yyyy-MM-dd}.txt");
                using (StreamWriter sw = File.AppendText(filePath))
                {
                    sw.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception ex)
            {
                // Handle file logging errors if necessary
                EventLog.WriteEntry("FileTransfer_WindowService", $"Error writing to log file: {ex.Message}", EventLogEntryType.Error);
            }
        }
    }
}
