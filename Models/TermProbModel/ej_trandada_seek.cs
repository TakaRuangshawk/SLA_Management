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
}