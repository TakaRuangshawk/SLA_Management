using System.ComponentModel.DataAnnotations;

namespace SLA_Management.Models
{
    public class P_Search
    {
        
        public string term_id { get; set; }
        [Required(ErrorMessage = "DateTime is required")]
        [DataType(DataType.Date)]
        public DateTime? toDateTime { get; set; }
        [Required(ErrorMessage = "DateTime is required")]
        [DataType(DataType.Date)]
        public DateTime? forDateTime { get; set; }
    }
}
