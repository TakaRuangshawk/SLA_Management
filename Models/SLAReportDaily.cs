using System.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SLA_Management.Models
{

    public class SLAReportDaily
    {
        public long ID { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? Open_Date { get; set; }
        public DateTime? Appointment_Date { get; set; }
        public DateTime? Closed_Repair_Date { get; set; }
        public string Down_Time { get; set; }
        public DateTime? AS_OpenDate { get; set; }
        public DateTime? AS_AppointmentDate { get; set; }
        public DateTime? AS_CloseRepairDate { get; set; }
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

                ID = row.Field<long>("ID") == null ? 0 : row.Field<long>("ID"),
                Open_Date = row.Field<DateTime?>("Open_Date") == null ? DateTime.Today : row.Field<DateTime>("Open_Date"),
                Appointment_Date = row.Field<DateTime?>("Appointment_Date") == null ? null : row.Field<DateTime?>("Appointment_Date"),
                Closed_Repair_Date = row.Field<DateTime?>("Closed_Repair_Date") == null ? null : row.Field<DateTime?>("Closed_Repair_Date"),
                Down_Time = row.Field<string>("Down_Time") == null ? null : row.Field<string>("Down_Time"),
                AS_OpenDate = row.Field<DateTime?>("AS_OpenDate") == null ? DateTime.Today : row.Field<DateTime>("AS_OpenDate"),
                AS_AppointmentDate = row.Field<DateTime?>("AS_AppointmentDate") == null ? null : row.Field<DateTime?>("AS_AppointmentDate"),
                AS_CloseRepairDate = row.Field<DateTime?>("AS_CloseRepairDate") == null ? null : row.Field<DateTime?>("AS_CloseRepairDate"),
                AS_Downtime = row.Field<string>("AS_Downtime") == null ? null : row.Field<string>("AS_Downtime"),
                Discount = row.Field<string>("Discount") == null ? null : row.Field<string>("Discount"),
                Net_Downtime = row.Field<string>("Net_Downtime") == null ? null : row.Field<string>("Net_Downtime"),
                AS_Discription = row.Field<string>("AS_Discription") == null ? null : row.Field<string>("AS_Discription"),
                AS_CIT_Request = row.Field<string>("AS_CIT_Request") == null ? null : row.Field<string>("AS_CIT_Request"),
                AS_Service_PM = row.Field<string>("AS_Service_PM") == null ? null : row.Field<string>("AS_Service_PM"),
                Status = row.Field<string>("status") == null ? null : row.Field<string>("status"),
                Terminal_ID = row.Field<string>("TERM_ID") == null ? null : row.Field<string>("TERM_ID"),
                Model = row.Field<string>("Model") == null ? null : row.Field<string>("Model"),
                Serial_NO = row.Field<string>("TERM_SEQ") == null ? null : row.Field<string>("TERM_SEQ"),
                Province = row.Field<string>("Province") == null ? null : row.Field<string>("Province"),
                Location = row.Field<string>("Location") == null ? null : row.Field<string>("Location"),
                Problem_Detail = row.Field<string>("Problem_Detail") == null ? null : row.Field<string>("Problem_Detail"),
                Service_Team = row.Field<string>("Service_Team") == null ? null : row.Field<string>("Service_Team"),
                Solving_Program = row.Field<string>("Solving_Program") == null ? null : row.Field<string>("Solving_Program"),
                Contact_Name_Branch_CIT = row.Field<string>("Contact_Name_Branch_CIT") == null ? null : row.Field<string>("Contact_Name_Branch_CIT"),
                Open_By = row.Field<string>("Open_By") == null ? null : row.Field<string>("Open_By"),
                Remark = row.Field<string>("Remark") == null ? null : row.Field<string>("Remark"),
            }).ToList();
            return items;
        }
    }
}
