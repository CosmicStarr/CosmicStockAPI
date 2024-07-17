using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class ActualOrder
    {
        public ActualOrder()
        {
            
        }
        public ActualOrder(IEnumerable<OrderedProducts> orderedProducts,string email,UserAddress address,UserAddress billingAddress,string orderStatus, decimal subtotal, decimal tax,decimal trueTotal,string paymentId,decimal shipping ) 
        {
            this.OrderedProducts = orderedProducts;
            this.Email = email;
            ShippingAddress = address;
            BillingAddress = billingAddress;
            OrderStatus = orderStatus;
            this.Subtotal = subtotal;
            Tax = tax;
            TrueTotal = trueTotal;
            this.PaymentId = paymentId;
            ShippingHandlingPrice = shipping;
            
        }
        public int Id { get; set; }
        public Guid ActualCustomerOrderId { get; set; } = Guid.NewGuid();
        public string Email { get; set; }
        public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset OrderReceived { get; set; } = new DateTimeOffset();
        public UserAddress ShippingAddress { get; set; }
        public UserAddress BillingAddress { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal Subtotal { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal TrueTotal { get; set; }
        public IEnumerable<OrderedProducts> OrderedProducts { get; set; }
        public string OrderStatus { get; set; } 
        public string PaymentId { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        [Column(TypeName ="decimal(18,2)")]
        public decimal ShippingHandlingPrice { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal Tax { get; set; }
        public string TrackingNumber { get; set; }
        public Boolean CancelOrder { get; set; } = false;


        public string DateInformation(){
            return OrderDate.Date.ToShortDateString();
        }
        public string OrderReceivedInformation(){
            return OrderReceived.Date.ToShortDateString();
        }
    }
}