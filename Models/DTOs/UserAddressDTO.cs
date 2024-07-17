using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class UserAddressDTO
    {
        public int Id { get; set; }
        [Required(ErrorMessage="Your first name is required!")]
        public string FirstName { get; set; }
        [Required(ErrorMessage="Your last name is required!")]
        public string LastName { get; set; }
        [Required(ErrorMessage="An address is required!")]
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        [Required(ErrorMessage="Your city is required!")] 
        public string City { get; set; }
        [Required(ErrorMessage="Your state is required!")] 
        public string State { get; set; }
        [Required(ErrorMessage="Your zipcode is required!")]
        [DataType(DataType.PostalCode)]
        public string ZipCode { get; set; } 
        [Required(ErrorMessage="Your phone number is required!")] 
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        public string AddressType { get; set; }
        public string AppUser {get; set;}
        public bool SaveAddressInfo { get; set; } 
        public bool ShippingIsBilling { get; set; }
    }
}