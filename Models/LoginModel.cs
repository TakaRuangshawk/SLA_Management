using System.ComponentModel.DataAnnotations;

namespace SLA_Management.Models
{
    public class LoginModel
    {
       
        [Display(Name = "ID")]
        public string? ID { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public string? Name { get; set; }

       
        public string? Role { get; set; }



    }

}
