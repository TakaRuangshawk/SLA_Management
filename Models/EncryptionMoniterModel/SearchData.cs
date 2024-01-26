namespace SLA_Management.Models.EncryptionMoniterModel
{
    public class SearchData
    {
        public string serial_no { get; set; }
        public string encryption_version { get; set; }
        public string encryption_status { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public string agent_status { get; set; }
        public string sort { get; set; }
        public int maxRows { get; set; }
    }
}
