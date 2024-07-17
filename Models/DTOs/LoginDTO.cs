using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage ="Email Address is Required!")]
        [DataType(DataType.EmailAddress,ErrorMessage ="Only valid emails are allowed!")]
        [EmailAddressAttribute(ErrorMessage ="Only valid emails are allowed!")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is Required!")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string Token { get; set; }
        public bool EmailConfirmed { get; set; }
        #nullable enable
        public string? Role { get; set; } 
        #nullable disable
        public string JobDepartment { get; set; }
        public ICollection<IdentityError> LoginErrors { get; set; }
    }
}