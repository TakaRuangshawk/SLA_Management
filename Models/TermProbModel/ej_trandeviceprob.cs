using System;
using System.ComponentModel.DataAnnotations;

namespace SLA_Management.Models.TermProbModel
{
    public class ej_trandeviceprob
    {
       
        public int Seqno { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime TransactionDate { get; set; }

        public string BranchName { get; set; }

        
        public string TerminalID { get; set; }

        public string Location { get; set; }

        public string ProbName { get; set; }

        public string Remark { get; set; }
    }
}