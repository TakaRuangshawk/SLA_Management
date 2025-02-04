using SLA_Management.Models.OperationModel;

namespace SLA_Management.Models.Monitor
{
    public class DeviceFirmwareViewModel
    {
        public List<DeviceFirmware> DeviceFirmwareList { get; set; }
        public List<Device_info_record>? Device_Info_Records { get; set; }
        public List<TermType>? TerminalTypes { get; set; }
        public string? selectedTerminal { get; set; }
        public string? selectedTerminalType { get; set; }
        public int TotalRecords { get; set; } = 0;
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class DeviceFirmware
    {
        public int? No { get; set; }
        public string? Term_ID { get; set; }
        public string? Term_SEQ { get; set; }
        public string? Term_Name { get; set; }
        public string? PIN_Ver { get; set; }
        public string? IDC_Ver { get; set; }
        public string? PTR_Ver { get; set; }
        public string? BCR_Ver { get; set; }
        public string? SIU_Ver { get; set; }
        public string? Update_Date { get; set; }
    }
    public class TermType
    {
        public string? Terminal_Type { get; set; }
    }
    public class DeviceFirmwareExport
    {
        public string? termID { get; set; }
        public string? terminalType { get; set; }
        public string? sort { get; set; }
    }
}
