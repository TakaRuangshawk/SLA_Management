namespace Models.ManagementModel
{
    public class CardRetain
    {

        public string Location { get; set; }

        public string SerialNo { get; set; }

        public string TerminalID { get; set; }

        public string TerminalName { get; set; }

        public string CounterCode { get; set; }

        public string CardNo { get; set; }

        public string Date { get; set; }

        public string Reason { get; set; }

        public string Vendor { get; set; }

        public string ErrorCode { get; set; }

        public string InBankFlag { get; set; }

        public string CardStatus { get; set; }

        public string Telephone { get; set; }

        public string UpdateDate { get; set; }
        public string UpdateBy { get; set; }

    }
    public class LatestUpdateDate_record
    {
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
    }
}
