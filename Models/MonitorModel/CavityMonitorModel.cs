using System;

namespace SLA_Management.Models
{
    /// <summary>
    /// ข้อมูลที่ใช้แสดงในหน้า Cavity Note Monitor
    /// </summary>
    public class CavityMonitorModel
    {
        public int no { get; set; }

        // Serial No.
        public string term_seq { get; set; }

        // Terminal ID
        public string term_id { get; set; }

        // Terminal Name
        public string term_name { get; set; }

        // Terminal Type (2IN1 / LRM / LRM21.5 / RDM ...)
        public string term_type { get; set; }

        // จาก CRM9250HardwareInfo.ini (0 / 1 / -1=ไม่รู้/อ่านไม่ได้)
        public int cavity_note { get; set; }

        // จาก XdcCimConfig → มี value="4199" หรือไม่
        public bool xdc_has_4199 { get; set; }

        // จาก grg_update (UNVFW) → เวลา log ล่าสุด
        public DateTime? nv_log_time { get; set; }

        // จาก grg_update (UNVFW) → NV=...
        public string nv_version { get; set; }

        public string main_version { get; set; }
    }
}
