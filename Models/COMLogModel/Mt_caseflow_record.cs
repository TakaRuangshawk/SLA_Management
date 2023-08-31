using System.Data;

namespace SLA_Management.Models.COMLogModel
{
    public class Mt_caseflow_record
    {
        private string id;
        private string term_id;
        private string event_id;
        private DateTime start_time;
        private DateTime deal_time;
        private int deal_step;
        private DateTime next_deal_time;
        private DateTime end_time;
        private string rule_id;
        private DateTime arrive_time;
        private int max_step;
        private string source;
        private string maintenance_state;
        private string maintenance_remark;
        private string report_man;
        private string maintenance_man;
        private DateTime MAINTENANCE_TIME;
        private string fault_remark;
        private string mt_man_longitude;
        private string mt_man_latitude;


        public string Id { get => id; set => id = value; }
        public string Term_id { get => term_id; set => term_id = value; }
        public string Event_id { get => event_id; set => event_id = value; }
        public DateTime Start_time { get => start_time; set => start_time = value; }
        public DateTime Deal_time { get => deal_time; set => deal_time = value; }
        public int Deal_step { get => deal_step; set => deal_step = value; }
        public DateTime Next_deal_time { get => next_deal_time; set => next_deal_time = value; }
        public DateTime End_time { get => end_time; set => end_time = value; }
        public string Rule_id { get => rule_id; set => rule_id = value; }
        public DateTime Arrive_time { get => arrive_time; set => arrive_time = value; }
        public int Max_step { get => max_step; set => max_step = value; }
        public string Source { get => source; set => source = value; }
        public string Maintenance_state { get => maintenance_state; set => maintenance_state = value; }
        public string Maintenance_remark { get => maintenance_remark; set => maintenance_remark = value; }
        public string Report_man { get => report_man; set => report_man = value; }
        public string Maintenance_man { get => maintenance_man; set => maintenance_man = value; }
        public DateTime MAINTENANCE_TIME1 { get => MAINTENANCE_TIME; set => MAINTENANCE_TIME = value; }
        public string Fault_remark { get => fault_remark; set => fault_remark = value; }
        public string Mt_man_longitude { get => mt_man_longitude; set => mt_man_longitude = value; }
        public string Mt_man_latitude { get => mt_man_latitude; set => mt_man_latitude = value; }

        public static IList<Mt_caseflow_record> mapToList(DataTable list)
        {
            IList<Mt_caseflow_record> items = list.AsEnumerable().Select(row => new Mt_caseflow_record
            {
                Id = row.Field<string>("ID") == null ? null : row.Field<string>("ID"),
                Term_id = row.Field<string>("TERM_ID") == null ? null : row.Field<string>("TERM_ID"),
                Event_id = row.Field<string>("EVENT_ID") == null ? null : row.Field<string>("EVENT_ID"),
                Start_time = row.Field<DateTime?>("START_TIME") == null ? new DateTime() : row.Field<DateTime>("START_TIME"),
                Deal_time = row.Field<DateTime?>("DEAL_TIME") == null ? new DateTime() : row.Field<DateTime>("DEAL_TIME"),
                Deal_step = row.Field<int?>("DEAL_STEP") == null ? 0 : row.Field<int>("DEAL_STEP"),
                Next_deal_time = row.Field<DateTime?>("NEXT_DEAL_TIME") == null ? new DateTime() : row.Field<DateTime>("NEXT_DEAL_TIME"),
                End_time = row.Field<DateTime?>("END_TIME") == null ? new DateTime() : row.Field<DateTime>("END_TIME"),
                Rule_id = row.Field<string>("RULE_ID") == null ? null : row.Field<string>("RULE_ID"),
                Arrive_time = row.Field<DateTime?>("ARRIVE_TIME") == null ? new DateTime() : row.Field<DateTime>("ARRIVE_TIME"),
                Max_step = row.Field<int?>("MAX_STEP_NUM") == null ? 0 : row.Field<int>("MAX_STEP_NUM"),
                Source = row.Field<string>("SOURCE") == null ? null : row.Field<string>("SOURCE"),
                Maintenance_state = row.Field<string>("MAINTENANCE_STATE") == null ? null : row.Field<string>("MAINTENANCE_STATE"),
                Maintenance_remark = row.Field<string>("MAINTENANCE_REMARK") == null ? null : row.Field<string>("MAINTENANCE_REMARK"),
                Report_man = row.Field<string>("REPORT_MAN") == null ? null : row.Field<string>("REPORT_MAN"),
                Maintenance_man = row.Field<string>("MAINTENANCE_MAN") == null ? null : row.Field<string>("MAINTENANCE_MAN"),
                MAINTENANCE_TIME1 = row.Field<DateTime?>("MAINTENANCE_TIME") == null ? new DateTime() : row.Field<DateTime>("MAINTENANCE_TIME"),
                Fault_remark = row.Field<string>("FAULT_REMARK") == null ? null : row.Field<string>("FAULT_REMARK"),
                Mt_man_longitude = row.Field<string>("MT_MAN_LONGITUDE") == null ? null : row.Field<string>("MT_MAN_LONGITUDE"),
                Mt_man_latitude = row.Field<string>("MT_MAN_LATITUDE") == null ? null : row.Field<string>("MT_MAN_LATITUDE")
            }).ToList();
            return items;
        }
    }
}
