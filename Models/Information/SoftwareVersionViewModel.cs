using SLA_Management.Models.OperationModel;
using System.ComponentModel.DataAnnotations;

namespace SLA_Management.Models.Information
{
    public class SoftwareVersionViewModel
    {
        public List<Terminal_SerialList>? SerialList { get; set; } = new List<Terminal_SerialList>();
        public List<Software_VersionList>? SoftwareList { get; set; } = new List<Software_VersionList>();
        public List<SP_VersionList>? SPVersionList { get; set; } = new List<SP_VersionList>();
        public List<Feelview_VersionList>? FeelviewVersionList { get; set; } = new List<Feelview_VersionList>();
        public List<Device_info_record>? Device_Info_Records { get; set; } = new List<Device_info_record> ();
        public List<SoftwareDataTable>? SoftwareData { get; set; } = new List<SoftwareDataTable> ();
        public string? selectedSerial_No { get; set; }
        public string? selectedBank { get;set; }
        public string? selectedTerminal { get; set; }
        public string? selectedSoftware_Ver { get; set; }
        public int TotalRecords { get; set; } = 0;
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
    public class Terminal_SerialList
    {
        public string? Serial_No { get; set; }
    }
    public class Software_VersionList
    {
        public string? Software_Ver { get; set; }
    }
    public class SP_VersionList
    {
        public string? SP_Ver { get; set; }
    }
    public class Feelview_VersionList
    {
        public string? Feelview_Ver { get; set; }
    }
    public class RequestSoftwareModel
    {
        public string? bank_Name { get; set; }
        public string? term_ID { get; set; }
        public string? software_Val { get; set; }
        public string? sp_Val { get; set; }
        public string? feelview_Val { get; set; }
        public int maxRows { get; set; }
        public int page { get; set; }
    }
    public class SoftwareDataTable
    {
        public int No { get; set; }
        public string? Term_ID { get; set; }
        public string? Term_Name { get; set; }
        public string? Serial_No { get; set; }
        public string? ATMC_Ver { get; set; }
        public string? SP_Ver { get; set; }
        public string? Agent_Ver { get; set; }
    }
    public class ExportSoftwareVersion
    {
        public string? bank_Name { get; set; }
        public string? term_ID { get; set; }
        public string? software_Val { get; set; }
        public string? sp_Val { get; set; }
        public string? feelview_Val { get; set; }
    }
}
