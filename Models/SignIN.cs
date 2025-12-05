using System.ComponentModel.DataAnnotations;

namespace MARS_MELA.Models
{
    public class SignIN
    {
        [Required(ErrorMessage = "Mobile number required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile must be 10 digits")]
        public string MobileNo { get; set; }


    }
}
