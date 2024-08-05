using System.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SLA_Management.Models.ReportModel
{

    public class SLAReportDaily
    {
        public long ID { get; set; }
        public string Report_Date { get; set; }
        public string Open_Date { get; set; }
        public string Appointment_Date { get; set; }
        public string Closed_Repair_Date { get; set; }
        public string Down_Time { get; set; }
        public string AS_OpenDate { get; set; }
        public string AS_AppointmentDate { get; set; }
        public string AS_CloseRepairDate { get; set; }
        public string AS_Downtime { get; set; }
        public string Discount { get; set; }
        public string Net_Downtime { get; set; }
        public string AS_Discription { get; set; }
        public string AS_CIT_Request { get; set; }
        public string AS_Service_PM { get; set; }
        public string Status { get; set; }
        public string Terminal_ID { get; set; }
        public string Model { get; set; }
        public string Serial_NO { get; set; }
        public string Province { get; set; }
        public string Location { get; set; }
        public string Problem_Detail { get; set; }
        public string Service_Team { get; set; }
        public string Solving_Program { get; set; }
        public string Contact_Name_Branch_CIT { get; set; }
        public string Open_By { get; set; }
        public string Remark { get; set; }

        public static IList<SLAReportDaily> mapToList(DataTable list)
        {
            IList<SLAReportDaily> items = list.AsEnumerable().Select(row => new SLAReportDaily
            {
                ID = row.Field<long?>("ID") ?? 0,
                Report_Date = row.Field<string>("Report_Date"),
                Open_Date = row.Field<string>("Open_Date"),
                Appointment_Date = row.Field<string>("Appointment_Date"),
                Closed_Repair_Date = row.Field<string>("Closed_Repair_Date"),
                Down_Time = row.Field<string>("Down_Time"),
                AS_OpenDate = row.Field<string>("AS_OpenDate"),
                AS_AppointmentDate = row.Field<string>("AS_AppointmentDate"),
                AS_CloseRepairDate = row.Field<string>("AS_CloseRepairDate"),
                AS_Downtime = row.Field<string>("AS_Downtime"),
                Discount = row.Field<string>("Discount"),
                Net_Downtime = row.Field<string>("Net_Downtime"),
                AS_Discription = row.Field<string>("AS_Discription")?.Replace("\n", "").Replace("\r", "").Replace(",", "|"),
                AS_CIT_Request = row.Field<string>("AS_CIT_Request")?.Replace("\n", "").Replace("\r", "").Replace(",", "|"),
                AS_Service_PM = row.Field<string>("AS_Service_PM")?.Replace("\n", "").Replace("\r", "").Replace(",", "|"),
                Status = row.Field<string>("Status"),
                Terminal_ID = row.Field<string>("TERM_ID"),
                Model = row.Field<string>("Model"),
                Serial_NO = row.Field<string>("TERM_SEQ"),
                Province = row.Field<string>("Province"),
                Location = row.Field<string>("Location"),
                Problem_Detail = row.Field<string>("Problem_Detail")?.Replace(Environment.NewLine, " ").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ").Replace(",", "|").Replace("-", ">"),
                Service_Team = row.Field<string>("Service_Team")?.Replace("\n", "").Replace("\r", "").Replace(",", "|"),
                Solving_Program = row.Field<string>("Solving_Program")?.Replace("\n", "").Replace("\r", "").Replace(",", "|"),
                Contact_Name_Branch_CIT = row.Field<string>("Contact_Name_Branch_CIT")?.Replace("\n", "").Replace("\r", "").Replace(",", "|"),
                Open_By = row.Field<string>("Open_By")?.Replace(Environment.NewLine, " ").Replace("\n", "").Replace("\r", "").Replace(",", "|"),
                Remark = row.Field<string>("Remark"),
            }).ToList();

            return items;
        }
    }
    public class TicketManagement
    {
        public string Open_Date { get; set; }
        public string Appointment_Date { get; set; }
        public string Closed_Repair_Date { get; set; }
        public string Down_Time { get; set; }
        public string Actual_Open_Date { get; set; }
        public string Actual_Appointment_Date { get; set; }
        public string Actual_Closed_Repair_Date { get; set; }
        public string Actual_Down_Time { get; set; }
        public string Status { get; set; }
        public string TERM_ID { get; set; }
        public string TERM_SEQ { get; set; }
        public string TERM_NAME { get; set; }
        public string MODEL_NAME { get; set; }
        public string PROVINCE { get; set; }
        public string Problem_Detail { get; set; }
        public string Solving_Program { get; set; }
        public string Service_Team { get; set; }
        public string Contact_Name_Branch_CIT { get; set; }
        public string Open_By { get; set; }
        public string Remark { get; set; }
        public string Job_No { get; set; }
        public string Aservice_Status { get; set; }
        public string Service_Type { get; set; }
        public string Open_Name { get; set; }
        public string Assign_By { get; set; }
        public string Zone_Area { get; set; }
        public string Main_Problem { get; set; }
        public string Sub_Problem { get; set; }
        public string Main_Solution { get; set; }
        public string Sub_Solution { get; set; }
        public string Part_of_use { get; set; }
        public string TechSupport { get; set; }
        public string CIT_Request { get; set; }
        public string Terminal_Status { get; set; }


    }
    public class TicketJob
    {
        public string Job_No { get; set; }
    }
}
