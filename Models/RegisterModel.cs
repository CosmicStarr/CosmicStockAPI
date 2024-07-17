using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class RegisterModel
    {

        [Required(ErrorMessage ="Email address is required!")]
        [DataType(DataType.EmailAddress,ErrorMessage ="Only valid emails are allowed!")]
        [EmailAddressAttribute(ErrorMessage ="Only valid emails are allowed!")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is Required!")]
        [Compare("ConfirmPassword",ErrorMessage = "Passwords do not match!")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 8,  ErrorMessage = " Password length error: Minimum length is 8 characters. Maximum is 20!")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Confirm Password is Required!")]
        public string ConfirmPassword {get; set;}
        #nullable enable
        public string? Role { get; set; } 
        public string? JobDepartment { get; set; }
        #nullable disable
    }
}