namespace SLA_Management.Models.HomeModel
{
    public class TopErrorDevice
    {
        public string TopErrorDeviceNo { get; set; }
        public string TopErrorDeviceName { get; set; }

        public string TopErrorDeviceDescription { get; set; }

        public TopErrorDevice(string no ,string name, string description)
        {
            TopErrorDeviceNo = no;
            TopErrorDeviceName = name;
            TopErrorDeviceDescription = description;
        }

    }
}
