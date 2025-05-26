namespace SLA_Management.Models.LogMonitorModel
{
    public class file_upload_history
    {
        public Int64 ID { get; set; }

        public string TERM_ID { get; set; }

        public DateTime? UPLOAD_DATE { get; set; }

        public string UPDATE_BY { get; set; }

        public string REMARK { get; set; }

        public string FILE_NAME { get; set; }

        public DateTime? FILE_DATETIME { get; set; }

        private string? _step;
        public string STEP
        {
            get => _step ?? string.Empty;
            set => _step = value;
        }

        private string? _status;
        public string STATUS
        {
            get => _status ?? string.Empty;
            set => _status = value;
        }



    }
}
