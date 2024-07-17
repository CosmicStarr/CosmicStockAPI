using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class OrderDTO
    {
        public string Id { get; set; }
        public UserAddressDTO ShippingAddress { get; set; }
        public UserAddressDTO BillingAddress { get; set; }
        public decimal ShippingHandlingPrice { get; set; }
        public decimal Tax { get; set; }
    }
}