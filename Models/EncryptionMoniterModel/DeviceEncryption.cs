namespace SLA_Management.Models.EncryptionMoniterModel
{
    public class DeviceEncryption
    {
        
        public int id { get; set; }
        public string serial_no { get; set; }
        public string terminal_id { get; set; }
        public string terminal_name { get; set; }
        public string encryption_version { get; set; }
        public string encryption_status { get; set; }
        public string agent_status { get; set; }
        public DateTime update_datetime { get; set; }

    }
}
