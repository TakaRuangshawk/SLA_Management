using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SLA_Management.Models.TermProbModel
{
    public class ej_trandada_seek
    {
        public string TERMID { get; set; }
        public string FRDATE { get; set; }
        public string TODATE { get; set; }
        public string TRXTYPE { get; set; }
        public int PAGESIZE { get; set; }
        public string PROBNAME { get; set; }
        public string PROBKEYWORD { get; set; }
        public string MONTHPERIOD { get; set; }
        public string YEARPERIOD { get; set; }
        public string TERMINALTYPE { get; set; }
    }

    public class gateway_seek
    {
        public string TerminalNo { get; set; }
        public string PhoneOTP { get; set; }
        public string acctnoto { get; set; }
        public string trxtype { get; set; }
        public string UpdateStatus { get; set; }
        public int PAGESIZE { get; set; }
        public string FRDATE { get; set; }
        public string TODATE { get; set; }
    }
    public class regulator_seek
    {
        public string TerminalNo { get; set; }
        public string SerialNo { get; set; }
        public int PAGESIZE { get; set; }
        public string FRDATE { get; set; }
        public string TODATE { get; set; }
    }
    public class ejchecksize_seek
    {
        public string TerminalNo { get; set; }
        public string SerialNo { get; set; }
        public string TerminalType { get; set; }
        public string status { get; set; }
        public int PAGESIZE { get; set; }
        public string Hours { get; set; }
    }

    public class cavity_seek
    {
        /// <summary>
        /// รหัสตู้ / Terminal ID ที่จะค้นหา (ว่าง = ทุกตู้)
        /// </summary>
        public string TerminalNo { get; set; }

        /// <summary>
        /// ประเภทตู้ (A = 2IN1, L = LRM, R = CDM/RDM หรือ "" = All)
        /// </summary>
        public string TerminalType { get; set; }

        /// <summary>
        /// ค่าของ Cavity Note ที่ต้องการกรอง ("", "0", "1")
        /// "" = All, "0" = ไม่มี Cavity, "1" = มี Cavity
        /// </summary>
        public string CavityNote { get; set; }

        /// <summary>
        /// ชื่อฟิลด์ที่ใช้ sort เช่น "trxdatetime", "term_id", "branch_id", "term_seq"
        /// </summary>
        public string SortField { get; set; }

        /// <summary>
        /// จำนวน record ต่อหน้า (ใช้คู่กับ paging)
        /// </summary>
        public int PAGESIZE { get; set; }

        /// <summary>
        /// วันที่เริ่มต้น (เผื่อกรองตามวันที่ในอนาคต) – รูปแบบ "yyyy-MM-dd HH:mm:ss"
        /// ถ้าไม่ใช้ตอนนี้ปล่อย null/"" ได้
        /// </summary>
        public string FRDATE { get; set; }

        /// <summary>
        /// วันที่สิ้นสุด (เผื่อกรองตามวันที่ในอนาคต) – รูปแบบ "yyyy-MM-dd HH:mm:ss"
        /// </summary>
        public string TODATE { get; set; }
    }
}