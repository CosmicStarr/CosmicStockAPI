using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class UserInformation
    {
        public string Email { get; set; }
        public string CurrentPassword { get; set; }
        [Required(ErrorMessage = "Password is Required!")]
        [Compare("ReTypeNewPassword", ErrorMessage = "Passwords do not match!")]
        [DataType(DataType.Password)]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,20}$",ErrorMessage = "You must have at least one uppercase letter, one lowercase letter, one number and one special character")]
        [StringLength(20, MinimumLength = 8,  ErrorMessage = " Password length error: Minimum length is 8 characters, Maximum length is 20")]
        public string NewPassword {get; set;}
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "ReType Password is Required!")]
        [StringLength(20, MinimumLength = 8,  ErrorMessage = " Password length error: Minimum length is 8 characters")]
        public string ReTypeNewPassword { get; set; }
        public string Token { get; set; }
        public bool passwordInfo { get; set; }
    }
}