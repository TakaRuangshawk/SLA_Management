using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;

namespace SLA_Management.Models.ReportModel
{
    public class SLAReportMonthly
    {
        public string ID { get; set; }
        public string REPORT_MONTH { get; set; }
        public string TERM_ID { get; set; }
        public string TERM_SEQ { get; set; }
        public string LOCATION { get; set; }
        public string PROVINCE { get; set; }
        public string INSTALL_LOT { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string REPLENISHMENT_DATE { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string STARTSERVICE_DATE { get; set; }
        public string STARTSERVICEDAY { get; set; }
        public string TOTALSTARTSERVICEDAY { get; set; }
        public string SERVICE_GROUP { get; set; }
        public string SERVICE_DATE { get; set; }
        public string STARTSERVICEDATE { get; set; }
        public string TOTALSTARTSERVICEDATE { get; set; }
        public string SERVICETIME_PERDAY { get; set; }
        public string SERVICETIME_PERHOUR { get; set; }
        public string SERVICETIME_PERMINUTE { get; set; }
        public string TOTALDOWNTIME_HOUR { get; set; }
        public string TOTALDOWNTIME_MINUTE { get; set; }
        public string TOTALSERVICETIME_PERHOUR { get; set; }
        public string TOTALSERVICETIME_PERMINUTE { get; set; }
        public string PERCENTSLA { get; set; }
        public string SLA { get; set; }
        public string RATECHARGE { get; set; }
        public string SERVICECHARGE { get; set; }
        public string NETSERVICECHARGE { get; set; }
        public string REMARK { get; set; }
        public string REASONFORWAIVEDOWNTIME { get; set; }
        public string TERM_NAME { get; set; }

        public static IList<SLAReportMonthly> mapToList(DataTable list)
        {
            IList<SLAReportMonthly> items = list.AsEnumerable().Select(row => new SLAReportMonthly
            {
                ID = row.Field<long?>("ID")?.ToString() ?? "-",
                REPORT_MONTH = row.Field<string>("REPORT_MONTH") ?? "-",
                TERM_ID = row.Field<string>("TERM_ID") ?? "-",
                TERM_SEQ = row.Field<string>("TERM_SEQ") ?? "-",
                LOCATION = row.Field<string>("LOCATION") ?? "-",
                PROVINCE = row.Field<string>("PROVINCE") ?? "-",
                INSTALL_LOT = row.Field<string>("INSTALL_LOT") ?? "-",
                REPLENISHMENT_DATE = row.Field<string>("REPLENISHMENT_DATE") ?? "-",
                STARTSERVICE_DATE = row.Field<string>("STARTSERVICE_DATE") ?? "-",
                STARTSERVICEDAY = row.Field<string>("STARTSERVICEDAY") ?? "-",
                TOTALSTARTSERVICEDAY = row.Field<string>("TOTALSTARTSERVICEDAY") ?? "-",
                SERVICE_GROUP = row.Field<string>("SERVICE_GROUP") ?? "-",
                SERVICE_DATE = row.Field<string>("SERVICE_DATE") ?? "-",
                STARTSERVICEDATE = row.Field<string>("STARTSERVICEDATE") ?? "-",
                TOTALSTARTSERVICEDATE = row.Field<string>("TOTALSTARTSERVICEDATE") ?? "-",
                SERVICETIME_PERDAY = row.Field<string>("SERVICETIME_PERDAY") ?? "-",
                SERVICETIME_PERHOUR = row.Field<string>("SERVICETIME_PERHOUR") ?? "-",
                SERVICETIME_PERMINUTE = row.Field<string>("SERVICETIME_PERMINUTE") ?? "-",
                TOTALDOWNTIME_HOUR = row.Field<string>("TOTALDOWNTIME_HOUR") ?? "-",
                TOTALDOWNTIME_MINUTE = row.Field<string>("TOTALDOWNTIME_MINUTE") ?? "-",
                TOTALSERVICETIME_PERHOUR = row.Field<string>("TOTALSERVICETIME_PERHOUR") ?? "-",
                TOTALSERVICETIME_PERMINUTE = row.Field<string>("TOTALSERVICETIME_PERMINUTE") ?? "-",
                PERCENTSLA = row.Field<string>("PERCENTSLA") ?? "-",
                SLA = row.Field<string>("SLA") ?? "-",
                RATECHARGE = row.Field<string>("RATECHARGE") ?? "-",
                SERVICECHARGE = row.Field<string>("SERVICECHARGE") ?? "-",
                NETSERVICECHARGE = row.Field<string>("NETSERVICECHARGE") ?? "-",
                REMARK = row.Field<string>("REMARK") ?? "-",
                REASONFORWAIVEDOWNTIME = row.Field<string>("REASONFORWAIVEDOWNTIME") ?? "-",
                TERM_NAME = row.Field<string>("TERM_NAME") ?? "-"
            }).ToList();
            return items;
        }
    }
}
