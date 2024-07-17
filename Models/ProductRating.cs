using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class ProductRating
    {
        public int Id { get; set; }
        public int ActualRating { get; set; }
        public string User { get; set; }
        [ForeignKey("ProductsId")]
        public int ProductsId { get; set; }
    }
}