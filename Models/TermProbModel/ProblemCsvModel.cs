namespace SLA_Management.Models.TermProbModel
{
    public class ProblemCsvModel
    {

        public string? terminalid { get; set; }
        public string? probcode { get; set; }
        public string? remark { get; set; }
        public string? trxdatetime { get; set; } // <-- string
        public string? createdate { get; set; }
        public string? updatedate { get; set; }
        public string? dtenderrcode13 { get; set; }
        public string? dterrcode13 { get; set; }
        public string? status { get; set; }
        public string? resolveprob { get; set; }
    }
}
