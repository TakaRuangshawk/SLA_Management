using Renci.SshNet.Sftp;
using Renci.SshNet;
using Serilog;

namespace SLA_Management.Commons
{
    public class ServiceFTP
    {
        private SftpClient sftp;

        public ServiceFTP(string IP, int port, string username, string password)
        {
            this.sftp = new SftpClient(IP, port, username, password);

            Connect();
        }
        public void Disconnect()
        {
            sftp.Disconnect();
        }
        public void Connect()
        {
            if (!sftp.IsConnected)
            {
                sftp.Connect();
            }
        }

        public bool DownloadFile(string pathDownloadFileServer, string pathSaveFile)
        {
            if (sftp.Exists(pathDownloadFileServer))
            {
                try
                {
                    using (Stream file = File.OpenWrite(pathSaveFile))
                    {
                        sftp.DownloadFile(pathDownloadFileServer, file);
                        file.Close();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"DownloadFile Error : {ex}");
                    return false;
                }

            }
            else
            {
                Log.Error($"DownloadFile Error : file not found");
                return false;
            }

        }
        public List<SftpFile> GetListFileToDate(string path, DateTime date)
        {
            var logDate = date.Year.ToString("0000") + date.Month.ToString("00") + date.Day.ToString("00");
            var files = GetListFile(path).Where(q => q.Name.Contains(logDate)).ToList();
            return files;

        }
        public List<SftpFile> GetListFile(string path)
        {
            if (sftp.Exists(path))
            {
                //List<SftpFile> files = sftp.ListDirectory(path).Where(q => q.Name != "." || q.Name != "..").ToList();
                List<SftpFile> files = sftp.ListDirectory(path)
                 .Where(q => q.Name != "." && q.Name != "..")
                 .Cast<SftpFile>()
                 .ToList();

                return files;
            }
            else
            {
                return new List<SftpFile>();
            }

        }


        public void UploadFile(string pathFile, string pathFileServer)
        {
            try
            {
                if (File.Exists(pathFile))
                {
                    using (Stream file = new FileStream(pathFile, FileMode.Open))
                    {
                        sftp.BufferSize = 1024;
                        sftp.UploadFile(file, pathFileServer);
                        file.Close();
                    }
                }
                else
                {
                    Log.Error("UploadFile error " + pathFile + " : Could not find file.");
                }
            }
            catch (Exception ex)
            {
                Log.Error("UploadFile error " + pathFile + " : " + ex);

            }

        }


        public void CreateDirectory(string pathFileServer)
        {
            try
            {
                sftp.CreateDirectory(pathFileServer);
            }
            catch (Exception ex)
            {
                Log.Error($"CreateDirectory Error : {ex}");
            }

        }

        public bool CheckDirectory(string pathFileServer)
        {
            try
            {
                return sftp.Exists(pathFileServer);
            }
            catch (Exception ex)
            {
                Log.Error($"CheckDirectory Error : {ex}");
                return false;
            }

        }



    }
}
