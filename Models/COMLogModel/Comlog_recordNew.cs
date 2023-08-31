namespace SLA_Management.Models.COMLogModel
{
    public class Comlog_recordNew
    {
        public int ID { get; set; }
        public string TERM_ID { get; set; }
        public string MSG_SOURCE { get; set; }
        public DateTime UPDATE_DATE { get; set; }
        public string UPDATE_BY { get; set; }
        public string REMARK { get; set; }
        public string ERROR { get; set; }
        public int TOTAL_RECORD { get; set; }
        public DateTime COMLOGDATE { get; set; }
        public int FLAG { get; set; }
    }
}
