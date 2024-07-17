using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class EmailChangeModel
    {
        [Required(ErrorMessage ="Email address is required!")]
        [DataType(DataType.EmailAddress,ErrorMessage ="Only valid emails are allowed!")]
        [EmailAddressAttribute(ErrorMessage ="Only valid emails are allowed!")]
        [Compare("ConfirmNewEmail",ErrorMessage = "Emails do not match!")]
        public string NewEmail { get; set; }
        [Required(ErrorMessage ="Email address is required!")]
        [DataType(DataType.EmailAddress,ErrorMessage ="Only valid emails are allowed!")]
        [EmailAddressAttribute(ErrorMessage ="Only valid emails are allowed!")]
        public string ConfirmNewEmail { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "A password is Required!")]
        [StringLength(20, MinimumLength = 8,  ErrorMessage = " Password length error: Minimum length is 8 characters. Maximum is 20!")]
        public string CurrentPassword { get; set; }
        public string token { get; set; }
    }
}