using System.ComponentModel.DataAnnotations;

namespace MARS_MELA.Models
{
    public class SignUP
    {

        [Required(ErrorMessage = "Mobile number required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile must be 10 digits")]
        public string MobileNo { get; set; }



        [Required(ErrorMessage = "Email ID is required")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",ErrorMessage = "Enter a valid email address")]
        public string EmailID { get; set; }


        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50, ErrorMessage = "First Name cannot be longer than 50 characters")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "First Name can contain only alphabets")]
        public string FirstName { get; set; }



        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50, ErrorMessage = "Last Name cannot be longer than 50 characters")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Last Name can contain only alphabets")]
        public string LastName { get; set; }


        [Required(ErrorMessage = "Please select who created the account")]
        public string CreatedBy { get; set; }




    }
}
