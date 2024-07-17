namespace Models
{
    public class Products
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FeaturedName { get; set; }
        public string Description { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal MSRP { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal price { get; set; }
        public bool? isAvailable { get; set; }
        public bool? isFeatured {get; set;}
        public bool? isOnSale { get; set; }
        public ICollection<Photo> imageUrl { get; set; }
        public ICollection<ProductRating> Ratings { get; set; }
        public Category Category { get; set; }
        public ProductDetails Details { get; set; }
        public Brand Brand { get; set; }
        public int AvailableAmount { get; set; } = 0;
        public int? TotalRate { get; set; } = 0;

        public bool isAvailableToBuy()
        {
            if(AvailableAmount is 0)
            {
                return isAvailable ?? false;
            }
            return true;
        }
        public decimal savings() 
        {
            return MSRP - price;
        }
    }
}