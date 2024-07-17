using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class Post
    {
        public string Name { get; set; }
        public string FeaturedName { get; set; }
        public string Description { get; set; }
        public string ActualDetails { get; set; }
        public string ActualDetails1 { get; set; }
        public string ActualDetails2 { get; set; }
        public string ActualDetails3 { get; set; }
        public string ActualDetails4 { get; set; }
        public string ActualDetails5 { get; set; }
        public string ActualDetails6 { get; set; }
        public string ActualDetails7 { get; set; }
        public string ActualDetails8 { get; set; }
        public string ActualDetails9 { get; set; }
        public int AvailableAmount { get; set; } 
        public ICollection<IFormFile> file { get; set; }
        public decimal MSRP { get; set; }
        public decimal price { get; set; }
        public bool? isAvailable { get; set; }
        public bool? isFeatured {get; set;}
        public bool? isOnSale { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }
    }
}