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
    }
}
