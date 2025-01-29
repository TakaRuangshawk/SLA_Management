using SLA_Management.Models.OperationModel;

namespace SLA_Management.Models.RecurringCasesMonitor
{
    public class RecurringCasesMonitorViewModel
    {
        public List<RecurringCase>? RecurringCases {  get; set; }
        public RecurringCase? RecurringCase { get; set; }
        public List<Device_info_record>? Device_Info_Records { get; set; }
        public List<TerminalType>? TerminalTypes { get; set; }
        public List<RecurringCaseDetail>? RecurringCaseDetails { get; set; }
        public string? selectedTerminalType { get; set; }   // purpose to keep selected value
        public string? selectedTerminal { get; set; }   // purpose to keep selected value
        public string? orderBy { get; set; }   // purpose to keep selected value
        public int TotalRecords { get; set; } = 0;
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public string? frDate { get;set; } // purpose to select detail later
        public string? toDate { get;set; } // purpose to select detail later
        public string? bank { get;set; } // purpose to select detail later
    }
    public class RecurringCase
    {
        public int? No { get; set; }
        public string? Serial_No { get; set; }
        public string? Terminal_Id { get; set; }
        public string? Terminal_Name { get; set; }
        public string? Counter_Code { get; set; }
        public int? Total_Recurring_Cases { get; set; }        
    }
    public class RecurringCaseDetail
    {
        public string? Date_Inform { get; set; }
        public string? Case_Error_No { get; set; }
        public string? Issue_Name { get; set; }
        public string? Repair1 { get; set; }
        public string? Repair2 { get; set; }
        public string? Incident_No { get; set; }
    }
    public class TerminalType
    {
        public string? Terminal_Type { get; set; }
    }
    public class ExportModel
    {
        public string? bankName { get; set; }
        public string? termID { get; set; }
        public string? frDate { get; set; }
        public string? toDate { get; set; }
        public string? terminalType { get; set; }
        public string? orderBy { get; set; }
    }
}
