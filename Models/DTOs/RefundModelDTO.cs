using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class RefundModelDTO
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public string ProductName { get; set; }
        public int ProductId { get; set; }
        public decimal amount { get; set; }
        public string ReasonForRefund { get; set; }
        public Boolean? IsRefundAGo { get; set; }
        public string AdditionalNotes { get; set; }
        public Boolean? CancelRefund { get; set; }
    }
}