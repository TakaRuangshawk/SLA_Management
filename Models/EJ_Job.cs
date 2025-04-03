namespace SLA_Management.Models
{
    public class EJ_Job
    {     
        public string Job_ID { get; set; }

        public DateTime UploadDate { get; set; }

        public string UploadBy { get; set; }

        public string Status { get; set; }

        public int CountFile { get; set; }

    }
}
