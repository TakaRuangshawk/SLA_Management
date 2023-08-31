using System.ComponentModel;

namespace SLA_Management.Models.COMLogModel
{
    public class InsertListFileComLog
    {
        public InsertListFileComLog(string term_ID, string serialNo, string terminalName, string comLog, string fileServer, bool statusFile, string tOTAL_RECORD)
        {

            Term_ID = term_ID;
            SerialNo = serialNo;
            TerminalName = terminalName;
            ComLog = comLog;
            FileServer = fileServer;
            StatusFile = statusFile;
            TOTAL_RECORD = tOTAL_RECORD;
        }

        public InsertListFileComLog()
        {
        }

        [DisplayName("No.")]
        public int Id { get; set; } = 0;
        [DisplayName("Term ID")]
        public string Term_ID { get; set; }

        [DisplayName("Serial No.")]
        public string SerialNo { get; set; }
        [DisplayName("Terminal Name")]
        public string TerminalName { get; set; }


        [DisplayName("ComLog")]
        public string ComLog { get; set; }
        [DisplayName("File Server")]
        public string FileServer { get; set; }
        [DisplayName("Status File")]
        public bool StatusFile { get; set; }

        [DisplayName("Total record")]
        public string TOTAL_RECORD { get; set; }
    }
}
