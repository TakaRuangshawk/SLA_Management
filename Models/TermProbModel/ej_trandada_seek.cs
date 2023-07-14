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
}