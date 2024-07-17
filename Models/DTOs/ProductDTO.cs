using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FeaturedName { get; set; }
        public string Description { get; set; }
        public ProductDetailsDTO Details { get; set; }
        public ICollection<PhotoDTO> imageUrl { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal MSRP { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal price { get; set; }
        public decimal Savings { get; set; }
        public bool? isAvailable { get; set; }
        public bool? isFeatured {get; set;}
        public bool? isOnSale { get; set; }
        public ICollection<ProductRatingDTO> Ratings { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }
        public int AvailableAmount { get; set; } 
        public ICollection<IFormFile> file { get; set; }
        public int? TotalRate { get; set; }
    }
}