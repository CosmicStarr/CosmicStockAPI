using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class OrderedProductsDTO
    {
        public int Id { get; set; }
        public int ActualOrderId { get; set; }
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        [Range(0.1,double.MaxValue,ErrorMessage = "Price must be greater than 0!")]
        public decimal price { get; set; }
        [Range(0.1,double.MaxValue,ErrorMessage = "Amount must be greater than 0!")]
        public int Amount { get; set; }
        public string imageUrl { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }
        public string AppUser { get; set; }
        public Boolean? CancelItem { get; set; }
        public Boolean? RequestRefund { get; set; }
        public bool TimeToKeepCancelButtonOn { get; set; }
        public bool TimeToKeepRefundButtonOn { get; set; }
        public DateTimeOffset TimeOrderWasReceived { get; set; } 
    }
}