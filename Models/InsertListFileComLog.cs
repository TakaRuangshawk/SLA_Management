using System.ComponentModel;

namespace SLA_Management.Models
{
    public class InsertListFileComLog
    {
        [DisplayName("No.")]
        public int Id { get; set; } = 0;
        [DisplayName("Term ID")]
        public string Term_ID { get; set; }
        [DisplayName("ComLog")]
        public string ComLog { get; set; }
        [DisplayName("File Server")]
        public string FileServer { get; set; }
        [DisplayName("Total record")]
        public string TOTAL_RECORD { get; set; }
    }
}
