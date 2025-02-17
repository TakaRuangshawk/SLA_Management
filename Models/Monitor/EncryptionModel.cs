namespace SLA_Management.Models.Monitor
{
    public class EncryptionModel
    {
        public string Terminal_SEQ { get; set; }
        public string Terminal_ID { get; set; }
        public string Terminal_NAME { get; set; }
        public string Counter_Code { get; set; }
        public string Version { get; set; }
        public string Policy { get; set; }

    }
    public class Version_info_record
    {
        public string Term_Seq { get; set; }
        public string SecureAge_Version { get; set; }
        public string Policy { get; set; }

    }
    public class LatestUpdate_record
    {
        public DateTime Update_Date { get; set; }
        public string Update_By { get; set; }
    }
}
