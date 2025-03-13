namespace SLA_Management.Models
{
    public class EJournalModel
    {
        public string ID { get; set; }
        public string TerminalID { get; set; }

        public string SerialNo { get; set; }

        public string TerminalName { get; set; }

        public string TerminalType { get; set; }

        public string FileName { get; set; }

        public string FileContent { get; set; }

        public string UpdateDate { get; set; }

        public string LastUploadingTime { get; set; }

        public string UploadStatus { get; set; }

        public string FileLength { get; set; }

        public string pathOfFile { get; set; }

    }
}
