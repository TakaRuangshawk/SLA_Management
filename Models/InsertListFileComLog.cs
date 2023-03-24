namespace SLA_Management.Models
{
    public class InsertListFileComLog
    {
        public int Id { get; set; } = 0;
        public string TERM_ID { get; set; }
        public string ComLog { get; set; }
        public string FileServer { get; set; }
        public string TOTAL_RECORD { get; set; }
    }
}
