using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class ShoppingCart
    {
        public ShoppingCart()
        {
            
        }
        public ShoppingCart(string shopid)
        {
            Id = shopid;
        }
        
        [Key]
        public string Id { get; set; }
        public List<CartItems> ShoppingCartItems { get; set; } = new();
        public string ClientSecret { get; set; }
        public string PaymentID { get; set; }
    }
}