namespace SLA_Management.Models.LogMonitorModel
{
    public class logmonitorjob
    {
        public string Job_ID { get; set; }

        public string? Job_Type { get; set; }

        public DateTime? UploadDate { get; set; }

        public DateTime? CloseJobDate { get; set; }

        public string? UploadBy { get; set; }

        public string? Status { get; set; }

        public int? CountFile { get; set; }

        public string? TerminalID { get; set; }

    }
}
