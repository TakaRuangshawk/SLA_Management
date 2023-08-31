using System.Data;

namespace SLA_Management.Models.COMLogModel
{
    public class Device_info_his
    {
        private int no;
        private string device_id;
        private string term_id;
        private string term_seq;
        private string status;
        private string term_name;
        private string install_date;
        private string active_date;
        private string service_begindate;
        private string service_enddate;
        private string submit_date;


        public int No { get => no; set => no = value; }

        public string Device_id { get => device_id; set => device_id = value; }
        public string Term_id { get => term_id; set => term_id = value; }
        public string Term_seq { get => term_seq; set => term_seq = value; }
        public string Status { get => status; set => status = value; }
        public string Term_name { get => term_name; set => term_name = value; }
        public string Install_date { get => install_date; set => install_date = value; }
        public string Active_date { get => active_date; set => active_date = value; }
        public string Service_begindate { get => service_begindate; set => service_begindate = value; }
        public string Service_enddate { get => service_enddate; set => service_enddate = value; }
        public string Submit_date { get => submit_date; set => submit_date = value; }

        public static IList<Device_info_his> mapToList(DataTable list)
        {
            IList<Device_info_his> items = list.AsEnumerable().Select(row => new Device_info_his
            {
                Device_id = row.Field<string>("DEVICE_ID") == null ? null : row.Field<string>("DEVICE_ID"),
                Term_id = row.Field<string>("TERM_ID") == null ? null : row.Field<string>("TERM_ID"),
                Term_seq = row.Field<string>("TERM_SEQ") == null ? null : row.Field<string>("TERM_SEQ"),
                Status = row.Field<string>("STATUS") == null ? null : row.Field<string>("STATUS"),
                Term_name = row.Field<string>("TERM_NAME") == null ? null : row.Field<string>("TERM_NAME"),
                Install_date = row.Field<string>("INSTALL_DATE") == null ? null : row.Field<string>("INSTALL_DATE"),
                Active_date = row.Field<string>("ACTIVE_DATE") == null ? null : row.Field<string>("ACTIVE_DATE"),
                Service_begindate = row.Field<string>("SERVICE_BEGINDATE") == null ? null : row.Field<string>("SERVICE_BEGINDATE"),
                Service_enddate = row.Field<string>("SERVICE_ENDDATE") == null ? null : row.Field<string>("SERVICE_ENDDATE"),
                Submit_date = row.Field<string>("SUBMIT_DATE") == null ? null : row.Field<string>("SUBMIT_DATE")
            }).ToList();
            return items;
        }
    }
}
