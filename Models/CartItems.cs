using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class CartItems
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal price { get; set; }
        public int Amount { get; set; }
        public ICollection<Photo> imageUrl { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
    }
}