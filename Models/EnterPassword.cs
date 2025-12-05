using System.ComponentModel.DataAnnotations;

namespace MARS_MELA.Models
{
    public class EnterPassword
    {
        [Required(ErrorMessage = "Mobile number required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile must be 10 digits")]
        public string MobileNo { get; set; }


        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
       public string PasswordHash { get; set; }

    }
}
