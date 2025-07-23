using Newtonsoft.Json;

namespace SLA_Management.Models.OperationModel
{
    public class Device_info_record
    {
        [JsonProperty("TERM_ID")]
        public string TERM_ID { get; set; }
        public string COUNTER_CODE { get; set; }
        public string TYPE_ID { get; set; }

        [JsonProperty("TERM_SEQ")]
        public string TERM_SEQ { get; set; }

        [JsonProperty("TERM_NAME")]
        public string TERM_NAME { get; set; }
        
    }
}
