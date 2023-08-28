using System.ComponentModel.DataAnnotations;

namespace SLA_Management.Models.TermProbModel
{
    public class ej_terminalperoffline
    {
        public int seqno { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime trandate { get; set; }

        public string BranchName { get; set; }

        public string terminalid { get; set; }

        public string ipaddress { get; set; }

        public string location { get; set; }

        public string lasttimeupload { get; set; }

        public long downloadsize { get; set; }
    }
    public class ejloglastupdate
    {
        public string term_id { get; set; }
        public string term_seq { get; set; }
        public string term_name { get; set; }
        public string update_date { get; set; }
        public string lastran_date { get; set; }
        public string status { get; set; }
        public string terminalstatus { get; set; }
    }
    public class feelviewstatus
    {
        public string onlineATM { get; set; }
        public string onlineADM { get; set; }
        public string offlineATM { get; set; }
        public string offlineADM { get; set;}
        public string comlogATM { get; set; }
        public string comlogADM { get; set; }
    }
    public class comlogrecord
    {
        public string comlogADM { get; set; }
        public string comlogATM { get; set; }
    }
    public class slatracking
    {
        public string no { get; set; }
        public string APPNAME { get; set; }
        public string STATUS { get; set; }
        public string UPDATE_DATE { get; set; }
    }
    public class secone
    {
        public string _online { get; set; }
        public string _offline { get; set; }
    }
}
