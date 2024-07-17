using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class UserAddressToSaveOrNot
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id  { get; set; }   
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public State State { get; set; }
        [DataType(DataType.PostalCode)]
        public string ZipCode { get; set; } 
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        public string AddressType { get; set; } 
        public string AppUser { get; set; }
        public bool SaveAddressInfo { get; set; } 
        public bool ShippingIsBilling { get; set; }
    }
}