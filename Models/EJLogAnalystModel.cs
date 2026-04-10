namespace SLA_Management.Models
{
    public class EJLogAnalystModel
    {
        public long Id { get; set; }
        public string? TerminalId { get; set; }
        public DateTime? TransactionDateTime { get; set; }
        public string? TransactionType { get; set; }
        public string? TransactionStatus { get; set; }
        public decimal? Amount { get; set; }
        public string? CardNumber { get; set; }
        public string? SequenceNo { get; set; }
        public string FullTransaction { get; set; } = "";
        public string? EJFileName { get; set; }
        public string? TermSeq { get; set; }
        public string? TypeId { get; set; }
        public string? TermName { get; set; }


    }
    public class LatestUpdated_record
    {
        public DateTime Update_Date { get; set; }
        public string? Update_By { get; set; }
    }

    public class Transaction_status_record
    {
        public string? TerminalId { get; set; }
        public string? TransactionStatus { get; set; }
    }

    public class EJTransaction
    {
        public string? TerminalId { get; set; }
        public DateTime? TransactionDateTime { get; set; }
        public string? TransactionType { get; set; }
        public string? TransactionStatus { get; set; }
        public decimal? Amount { get; set; }
        public string? CardNumber { get; set; }
        public string? SequenceNo { get; set; }
        public string FullTransaction { get; set; } = "";
        public TimeOnly? TransactionTime { get; set; }

    }




}
