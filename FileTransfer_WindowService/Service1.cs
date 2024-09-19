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
            timer = new Timer(30000); // Poll every 30 seconds
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Start();
            WriteToFile("Service started.");
        }

        protected override void OnStop()
        {
            timer.Stop();
            WriteToFile("Service stopped.");
        }

        private void OnElapsedTime(object sender, ElapsedEventArgs e)
        {
            ProcessFilesInDirectory();
        }

        private void ProcessFilesInDirectory()
        {
            if (!Directory.Exists(sourceFolder))
            {
                WriteToFile($"Source folder does not exist: {sourceFolder}");
                return;
            }

            string[] files = Directory.GetFiles(sourceFolder, "*.txt");
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                var parts = fileName.Split(' ');
                if (parts.Length >= 3) // Ensure parts length is at least 3
                {
                    try
                    {
                        int channel = int.Parse(parts[0].Substring(1)); // Remove 'P' and parse channel
                        string datePart = parts[1]; // "01-10-2011"
                        string timePart = parts[2]; // "142536"
                        string formattedTime = timePart.Insert(2, ":").Insert(5, ":"); // "14:25:36"
                        DateTime dateTime = DateTime.ParseExact($"{datePart} {formattedTime}", "dd-MM-yyyy HH:mm:ss", null);

                        // Create the combined CallID from Channel, Date, and Time
                        string callId = $"{channel}-{dateTime:yyyyMMddHHmmss}"; // Format: Channel-YYYYMMDDHHMMSS

                        // Check if the CallID already exists in the database
                        if (IsCallIdExists(callId))
                        {
                            WriteToFile($"CallId {callId} already exists. Skipping database entry for file {fileName}.");
                        }
                        else
                        {
                            // Save to database using CallID
                            SaveToDatabase(callId, channel, dateTime);
                        }

                        // Move file to processed location
                        string destFile = Path.Combine(processedFolder, fileName);
                        File.Move(file, destFile);

                        WriteToFile($"File {fileName} processed and moved to {destFile}.");
                    }
                    catch (FormatException ex)
                    {
                        WriteToFile($"Invalid format in file {Path.GetFileName(file)}: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        WriteToFile($"Error processing file {Path.GetFileName(file)}: {ex.Message}");
                    }
                }
                else
                {
                    WriteToFile($"Unexpected file name format: {fileName}");
                }
            }
        }

        private bool IsCallIdExists(string callId)
        {
            string query = "SELECT COUNT(1) FROM ProcessedFiles_New WHERE CallId = @CallId";
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CallId", callId);

                try
                {
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0; // Return true if CallId already exists
                }
                catch (Exception ex)
                {
                    WriteToFile($"Error checking if CallId exists: {ex.Message}");
                    return false;
                }
            }
        }

        private void SaveToDatabase(string callId, int channel, DateTime dateTime)
        {
            string query = "INSERT INTO ProcessedFiles_New (CallId, Channel, Date, Time) VALUES (@CallId, @Channel, @Date, @Time)";
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CallId", callId);
                cmd.Parameters.AddWithValue("@Channel", channel);
                cmd.Parameters.AddWithValue("@Date", dateTime.Date);
                cmd.Parameters.AddWithValue("@Time", dateTime.TimeOfDay);

                try
                {
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    WriteToFile($"Inserted into ProcessedFiles_New - CallId: {callId}, Channel: {channel}, Date: {dateTime:yyyy-MM-dd}, Time: {dateTime:HH:mm:ss}, Rows Affected: {rowsAffected}");
                }
                catch (Exception ex)
                {
                    WriteToFile($"Error saving to database: {ex.Message}");
                }
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
