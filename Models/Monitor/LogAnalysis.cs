namespace SLA_Management.Models.Monitor
{
    public class LogAnalysisModel
    {
        public string BankCode { get; set; }
        public string Incident_No { get; set; }
        public string Terminal_SEQ { get; set; }
        public string Terminal_ID { get; set; }
        public string Terminal_NAME { get; set; }
        public string Incident_Date { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Analyst_Info { get; set; }
        public string Inform_By { get; set; }
        public string Counter_Code { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string TotalCase { get; set; }
    }
    public class Sisbu_analysis_record
    {
        public string TERM_ID { get; set; }
        public string incident_name { get; set; }

    }
    public class TotalCase_record
    {
        public string incident_name { get; set; }
        public string analyst_01 { get; set; }
        public string TotalCount { get; set; }
    }
    public class UpdateLogAnalysis
    {
        public string BankCode { get; set; }
        public string Incident_No { get; set; }
        public string Terminal_SEQ { get; set; }
        public string Terminal_ID { get; set; }
        public string Terminal_NAME { get; set; }
        public string Incident_Date { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Analyst_Info { get; set; }
        public string Inform_By { get; set; }
    }
}
