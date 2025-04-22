namespace SLA_Management.Models.LogMonitorModel
{
    public class fileLog_monitoring_cycle
    {
        public string Id { get; set; }

        public string terminal_id { get; set; }

        public DateTime date_process {  get; set; }
        
        public string file_name { get; set; }

        public int line_count { get; set; }

        public DateTime date_data { get; set; }

        public string? job_id { get; set; }


    }
}
