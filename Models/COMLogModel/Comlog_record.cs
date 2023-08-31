using System.Data;

namespace SLA_Management.Models.COMLogModel
{
    public class Comlog_record
    {
        private int id;
        private string term_id;
        private string msg_source;
        private int total_record;
        private DateTime update_date = DateTime.Now;
        private string update_by = "repairInsert";
        private string remark = "";

        public int Id { get => id; set => id = value; }
        public string Term_id { get => term_id; set => term_id = value; }
        public string Msg_source { get => msg_source; set => msg_source = value; }
        public int Total_record { get => total_record; set => total_record = value; }
        public DateTime Update_date { get => update_date; set => update_date = value; }
        public string Update_by { get => update_by; set => update_by = value; }
        public string Remark { get => remark; set => remark = value; }





        public override string ToString()
        {
            return term_id + "," + msg_source + "," + total_record + "," + update_date.ToString("yyyyMMdd") + "," + update_by + "," + remark;
        }
    }
}
