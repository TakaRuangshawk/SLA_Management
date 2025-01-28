using System.Reflection.Metadata;

namespace SLA_Management.Models
{
    public class HealthCheckModel
    {

        public string Terminal_ID { get; set; }

        public DateTime? Transaction_DateTime { get; set; }

        public string Terminal_Type { get; set; }

        public string Terminal_Name { get; set; }

        public string Status { get; set; }

        public string Serial_No { get; set; }

        public string Problem_ID { get; set; }


    }
}
