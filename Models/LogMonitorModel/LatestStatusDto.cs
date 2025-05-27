namespace SLA_Management.Models.LogMonitorModel
{
    public class LatestStatusDto
    {
        public int COUNT_ALL_TERMINAL { get; set; }

        public int COUNT_TERMINAL { get; set; }

        public int COUNT_TASK_TERMINAL { get; set; }

        public int COUNT_TASK_UPLOAD_SUCCESSFUL { get; set; }

        public int COUNT_UPLOAD_COMLOG_SUCCESSFUL { get; set; }

        public int COUNT_INSERT_COMLOG_SUCCESSFUL { get; set; }

        public int COUNT_TASK_UPLOAD_UNSUCCESSFUL { get; set; }

        public int COUNT_UPLOAD_COMLOG_UNSUCCESSFUL { get; set; }

        public int COUNT_INSERT_COMLOG_UNSUCCESSFUL { get; set; }
    }

}
