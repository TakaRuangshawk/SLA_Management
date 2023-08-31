namespace SLA_Management.Models.COMLogModel
{
    public class Sla_emslog
    {
        private string emsid;
        private string term_id;
        private DateTime start_datetime;
        private string event_no;
        private string id = "";
        private string msg_content;
        private string update_by = "FVEventMissingSystem";

        public string Term_id { get => term_id; set => term_id = value; }
        public DateTime Start_datetime { get => start_datetime; set => start_datetime = value; }
        public string Event_no { get => event_no; set => event_no = value; }
        public string Id { get => id; set => id = value; }
        public string Msg_content { get => msg_content; set => msg_content = value; }
        public string Update_by { get => update_by; set => update_by = value; }
        public string Emsid { get => emsid; set => emsid = value; }
    }
}
