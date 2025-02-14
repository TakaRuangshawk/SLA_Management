namespace SLA_Management.Models.Dashboard
{
    public class DashboradViewModel
    {
        public string? Serial_No { get; set; }
        public string? Terminal_Id { get; set; }
        public string? Terminal_Name { get;set; }
        public string? Counter_Code { get; set; }
        public int? Total_Cases { get; set; }
    }
    public class PieChartModel
    {
        public string? name { get; set; }
        public int? value { get; set; }
    }
}
