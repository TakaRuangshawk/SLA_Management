namespace SLA_Management.Models.LogMonitorModel
{
    public class file_upload_report
    {
        public Int64 ID { get; set; }

        public string LOG_DATE { get; set; }

        public string LOG_NAME { get; set; }

        public DateTime UPDATE_DATE { get; set; }

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
