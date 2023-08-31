using System.Data;

namespace SLA_Management.Models.COMLogModel
{
    public class Base24_master_errordesc
    {
        private string event_no;
        private string event_desc;
        private DateTime update_date;
        private string update_by;
        private string event_ams_default;
        private string event_type;
        private string feelview_eventino;
        private string feelview_event_type;

        public string Event_no { get => event_no; set => event_no = value; }
        public string Event_desc { get => event_desc; set => event_desc = value; }
        public DateTime Update_date { get => update_date; set => update_date = value; }
        public string Update_by { get => update_by; set => update_by = value; }
        public string Event_ams_default { get => event_ams_default; set => event_ams_default = value; }
        public string Event_type { get => event_type; set => event_type = value; }
        public string Feelview_eventNo { get => feelview_eventino; set => feelview_eventino = value; }
        public string Feelview_event_type { get => feelview_event_type; set => feelview_event_type = value; }
        public static IList<Base24_master_errordesc> mapToList(DataTable list)
        {
            IList<Base24_master_errordesc> items = list.AsEnumerable().Select(row => new Base24_master_errordesc
            {

                Event_no = row.Field<string>("Event_No") == null ? null : row.Field<string>("Event_No"),
                Event_desc = row.Field<string>("Event_DESC") == null ? null : row.Field<string>("Event_DESC"),
                Update_date = row.Field<DateTime?>("UPDATE_DATE") == null ? new DateTime() : row.Field<DateTime>("UPDATE_DATE"),
                Update_by = row.Field<string>("UPDATE_BY") == null ? null : row.Field<string>("UPDATE_BY"),
                Event_ams_default = row.Field<string>("EVENT_AMS_DEFAULT") == null ? null : row.Field<string>("EVENT_AMS_DEFAULT"),
                Feelview_eventNo = row.Field<string>("FEELVIEW_EVENTNO") == null ? null : row.Field<string>("FEELVIEW_EVENTNO"),
                Event_type = row.Field<string>("EVENT_TYPE") == null ? null : row.Field<string>("EVENT_TYPE"),
                Feelview_event_type = row.Field<string>("FEELVIEW_EVENT_TYPE") == null ? null : row.Field<string>("FEELVIEW_EVENT_TYPE"),
            }).ToList();
            return items;
        }
    }
}
