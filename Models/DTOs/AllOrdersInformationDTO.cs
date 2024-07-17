using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class AllOrdersInformationDTO
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string OrderDate { get; set; } 
        public string OrderReceived { get; set; } 
        public UserAddressDTO ShippingAddress { get; set; }
        public UserAddressDTO BillingAddress { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal Subtotal { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal TrueTotal { get; set; }
        public IEnumerable<OrderedProductsDTO> OrderedProducts { get; set; }
        public string OrderStatus { get; set; } 
        public string PaymentId { get; set; }
        public string PaymentStatus { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal ShippingHandlingPrice { get; set; }
        public string TrackingNumber { get; set; }
    }
}