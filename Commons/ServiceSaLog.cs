using Serilog;
using SLA_Management.Models.EncryptionMoniterModel;
using System.IO;

namespace SLA_Management.Commons
{
    public class ServiceSaLog : ServiceFTP
    {
        public string pathDirectory = "DownloadFile";
        public ServiceSaLog(string IP, int port, string username, string password) : base(IP, port, username, password)
        {
            if(!Directory.Exists(pathDirectory)) {
                Directory.CreateDirectory(pathDirectory);
            }
        }

        public List<LogEntry> ReadLog(string pathFileServer)
        {
            
            var pathFileDownloadFile = Path.Combine(pathDirectory, Guid.NewGuid().ToString() + ".log");
            if (DownloadFile(pathFileServer, pathFileDownloadFile))
            {

                List<LogEntry> logEntries = new List<LogEntry>();
                using (StreamReader file = new StreamReader(pathFileDownloadFile))
                {
                    int counter = 1;
                    string ln;

                    while ((ln = file.ReadLine()) != null)
                    {
                        try
                        {
                            var dataLog = new LogEntry(counter, ln);
                            logEntries.Add(dataLog);
                            counter++;


                        }
                        catch (Exception ex)
                        {
                            Log.Error($"ReadLog Error : {ex}");
                        }

                    }
                    file.Close();
                    
                }
                File.Delete(pathFileDownloadFile);
                return logEntries;
            }


            return new List<LogEntry>();
        }

    }
}
