using Renci.SshNet;
using SLA_Management.Models;

namespace SLA_Management.Commons
{
    public  class SendFileToServer
    {
        // Enter your host name or IP here
        private string host { get; set; }

        // Enter your sftp username here
        private string username { get; set; }
        // Enter your sftp password here
        private string password { get; set; }

        private  int port { get; set; }
        public SendFileToServer(string host, string username, string password, int port)
        {
            this.host = host;
            this.username = username;
            this.password = password;
            this.port = port;
        }


        public bool Send(UploadFileFrom file)
        {
            //var connectionInfo = new ConnectionInfo(host, "sftp", new PasswordAuthenticationMethod(username, password));

            // Upload File
            try
            {
                using (var sftp = new SftpClient(host, port, username, password))
                {

                    sftp.Connect();
                    //sftp.ChangeDirectory("/MyFolder");
                    using (var uplfileStream = System.IO.File.OpenRead(file.targetFile))
                    {
                        sftp.UploadFile(uplfileStream, file.pathUpload, true);
                    }
                    sftp.Disconnect();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            
           
        }
        public  void SendAll(UploadFileFrom[] fileName)
        {
            
            try
            {
                using (var sftp = new SftpClient(host, port, username, password))
                {

                    sftp.Connect();
                    //sftp.ChangeDirectory("/MyFolder");
                    foreach (var file in fileName)
                    {
                        using (var uplfileStream = System.IO.File.OpenRead(file.targetFile))
                        {
                            sftp.UploadFile(uplfileStream, file.pathUpload, true);
                        }
                    }
                    sftp.Disconnect();
                    
                }
            }
            catch (Exception ex)
            {
                
            }


        }
    }
}
