using System.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace SLA_Management.Models.ReportModel
{
    public class SLAReportMonthly
    {
        public long ID { get; set; }
        public string TERM_ID { get; set; }
        public string TERM_SEQ { get; set; }
        public string LOCATION { get; set; }
        public string PROVINCE { get; set; }
        public string INSTALL_LOT { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? REPLENISHMENT_DATE { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? STARTSERVICE_DATE { get; set; }
        public string TOTALSERVICEDAY { get; set; }
        public string SERVICE_GROUP { get; set; }
        public string SERVICE_DATE { get; set; }
        public string SERVICEDAY_CHARGE { get; set; }
        public string SERVICETIME_CHARGE_PERDAY { get; set; }
        public string SERVICETIME_PERMONTH_HOUR { get; set; }
        public string SERVICETIME_PERHOUR_MINUTE { get; set; }
        public string TOTALDOWNTIME_HOUR { get; set; }
        public string TOTALDOWNTIME_MINUTE { get; set; }
        public string ACTUAL_SERVICETIME_PERMONTH_HOUR { get; set; }
        public string ACTUAL_SERVICETIME_PERHOUR_MINUTE { get; set; }
        public string ACTUAL_PERCENTSLA { get; set; }
        public string RATECHARGE { get; set; }
        public string SERVICECHARGE { get; set; }
        public string NETCHARGE { get; set; }
        public string REMARK { get; set; }
        public string TERM_NAME { get; set; }
        public static IList<SLAReportMonthly> mapToList(DataTable list)
        {
            IList<SLAReportMonthly> items = list.AsEnumerable().Select(row => new SLAReportMonthly
            {
                ID = row.Field<long>("ID") == null ? 0 : row.Field<long>("ID"),
                TERM_ID = row.Field<string>("TERM_ID") == null ? null : row.Field<string>("TERM_ID"),
                TERM_SEQ = row.Field<string>("TERM_SEQ") == null ? " " : row.Field<string>("TERM_SEQ"),
                LOCATION = row.Field<string>("LOCATION") == null ? " " : row.Field<string>("LOCATION"),
                PROVINCE = row.Field<string>("PROVINCE") == null ? " " : row.Field<string>("PROVINCE"),
                INSTALL_LOT = row.Field<string>("INSTALL_LOT") == null ? " " : row.Field<string>("INSTALL_LOT"),
                REPLENISHMENT_DATE = row.Field<DateTime?>("REPLENISHMENT_DATE") == null ? null : row.Field<DateTime?>("REPLENISHMENT_DATE"),
                STARTSERVICE_DATE = row.Field<DateTime?>("STARTSERVICE_DATE") == null ? null : row.Field<DateTime?>("STARTSERVICE_DATE"),
                TOTALSERVICEDAY = row.Field<string>("TOTALSERVICEDAY") == null ? " " : row.Field<string>("TOTALSERVICEDAY"),
                SERVICE_GROUP = row.Field<string>("SERVICE_GROUP") == null ? " " : row.Field<string>("SERVICE_GROUP"),
                SERVICE_DATE = row.Field<string>("SERVICE_DATE") == null ? " " : row.Field<string>("SERVICE_DATE"),
                SERVICEDAY_CHARGE = row.Field<string>("SERVICEDAY_CHARGE") == null ? " " : row.Field<string>("SERVICEDAY_CHARGE"),
                SERVICETIME_CHARGE_PERDAY = row.Field<string>("SERVICETIME_CHARGE_PERDAY") == null ? " " : row.Field<string>("SERVICETIME_CHARGE_PERDAY"),
                SERVICETIME_PERMONTH_HOUR = row.Field<string>("SERVICETIME_PERMONTH_HOUR") == null ? " " : row.Field<string>("SERVICETIME_PERMONTH_HOUR"),
                SERVICETIME_PERHOUR_MINUTE = row.Field<string>("SERVICETIME_PERHOUR_MINUTE") == null ? " " : row.Field<string>("SERVICETIME_PERHOUR_MINUTE"),
                TOTALDOWNTIME_HOUR = row.Field<string>("TOTALDOWNTIME_HOUR") == null ? null : row.Field<string>("TOTALDOWNTIME_HOUR"),
                TOTALDOWNTIME_MINUTE = row.Field<string>("TOTALDOWNTIME_MINUTE") == null ? " " : row.Field<string>("TOTALDOWNTIME_MINUTE"),
                ACTUAL_SERVICETIME_PERMONTH_HOUR = row.Field<string>("ACTUAL_SERVICETIME_PERMONTH_HOUR") == null ? " " : row.Field<string>("ACTUAL_SERVICETIME_PERMONTH_HOUR"),
                ACTUAL_SERVICETIME_PERHOUR_MINUTE = row.Field<string>("ACTUAL_SERVICETIME_PERHOUR_MINUTE") == null ? " " : row.Field<string>("ACTUAL_SERVICETIME_PERHOUR_MINUTE"),
                ACTUAL_PERCENTSLA = row.Field<string>("ACTUAL_PERCENTSLA") == null ? " " : row.Field<string>("ACTUAL_PERCENTSLA"),
                RATECHARGE = row.Field<string>("RATECHARGE") == null ? " " : row.Field<string>("RATECHARGE"),
                SERVICECHARGE = row.Field<string>("SERVICECHARGE") == null ? " " : row.Field<string>("SERVICECHARGE"),
                NETCHARGE = row.Field<string>("NETCHARGE") == null ? " " : row.Field<string>("NETCHARGE"),
                REMARK = row.Field<string>("REMARK") == null ? " " : row.Field<string>("REMARK"),
                TERM_NAME = row.Field<string>("TERM_NAME") == null ? " " : row.Field<string>("TERM_NAME"),
            }).ToList();
            return items;
        }
    }

}
