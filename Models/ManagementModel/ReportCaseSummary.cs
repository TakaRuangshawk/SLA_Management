namespace SLA_Management.Models.ManagementModel
{
    public class ReportCaseSummary
    {
        public string IssueName { get; set; }
        public int[] BaacService { get; set; } = new int[12];
        public int[] AService { get; set; } = new int[12];
        public int TotalBaacService { get; set; }
        public int TotalAService { get; set; }
    }
}