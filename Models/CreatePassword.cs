using System.ComponentModel.DataAnnotations;

namespace MARS_MELA.Models
{
    public class CreatePassword
    {

        [Required(ErrorMessage = "Mobile number required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile must be 10 digits")]
        public string MobileNo { get; set; }



        [Required(ErrorMessage = "Email ID is required")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Enter a valid email address")]
        public string EmailID { get; set; }

        [Required(ErrorMessage = "Password is required")]
        //[DataType(DataType.Password)]

        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
               ErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string PasswordHash { get; set; }



        [Required(ErrorMessage = "Confirm Password is required")]
        //[DataType(DataType.Password)]
        [Compare("PasswordHash", ErrorMessage = "Password and Confirm Password do not match")]
        public string ConfirmPassword { get; set; }
    }
}
