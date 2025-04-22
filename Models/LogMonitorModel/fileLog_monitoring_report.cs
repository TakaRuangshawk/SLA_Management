namespace SLA_Management.Models.LogMonitorModel
{
    public class fileLog_monitoring_report
    {
        public int id {  get; set; }

        public string? terminal_id { get; set; }

        public string? terminal_Name { get; set; }

        public string? serial_No { get; set; }

        public DateTime? date { get; set; }

        public string? ejLog { get; set; }

        public string? eCatLog { get; set; }

        public string? comLog { get; set; }

        public string? imageLog { get; set; }

        public string? status {  get; set; }

        public string fileLog_monitoring_cycle_id { get; set; }

        public string? job_id { get; set; }

    }
}
