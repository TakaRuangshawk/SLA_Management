public class SudoVersionRecord
{
    public string serial_no { get; set; }        // fv_device_info.TERM_SEQ
    public string terminal_no { get; set; }      // fv_device_info.TERM_ID
    public string terminal_name { get; set; }    // fv_device_info.TERM_NAME
    public DateTime? last_updated { get; set; }   // sudo_version.last_update_datetime
    public string status { get; set; }           // sudo_version.status
}