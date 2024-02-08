using Microsoft.Identity.Client;

namespace SLA_Management.Models.ReportModel
{
    public class fv_system_users
    {

        public string System {  get; set; }

        public string AccountName {  get; set; }
         
        public string UserName { get; set; }

        public DateTime LastLoginDateTime { get; set; }

        public string Status {  get; set; }


    }
}
