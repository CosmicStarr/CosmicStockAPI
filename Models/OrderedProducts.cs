namespace Models
{
    public class OrderedProducts
    {
        public OrderedProducts()
        {
            
        }
        public OrderedProducts(
            int id, 
            string name, 
            string description, 
            decimal Price,
            int amount,
            ICollection<PhotosForOrderedProducts> pics,
            string category,
            string brand,
            string appUser)
        {
            ItemId = id;
            Name = name;
            Description = description;
            price = Price;
            Amount = amount;
            imageUrl = pics;
            Category = category;
            Brand = brand;
            AppUser = appUser;
        }
        public int Id { get; set; }
        [ForeignKey("ActualOrderId")]
        public int ActualOrderId { get; set; }
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal price { get; set; }
        public int Amount { get; set; }
        public ICollection<PhotosForOrderedProducts> imageUrl { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }
        public string AppUser { get; set; }
        public Boolean? CancelItem { get; set; }
        public Boolean? RequestRefund { get; set; } 
        public Boolean TimeToKeepRefundButtonOn {get; set;} = true;
        public Boolean TimeToKeepCancelButtonOn { get; set; } = true;
        public DateTimeOffset TimeOrderWasReceived { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset OrderedItemTime { get; set; } = DateTimeOffset.Now;
        public bool FifteenMinutesPassed()
        {
            var timeInfo = DateTimeOffset.Now - OrderedItemTime;
            if(timeInfo.Duration().TotalMinutes > 15 || CancelItem == true)
            {
                return TimeToKeepCancelButtonOn = false;
            }
            return true;
        }
        public bool OverThirtyDaysPassed()
        {
            var timeInfo = DateTimeOffset.Now - TimeOrderWasReceived;
            if(timeInfo.Duration().Days > 30)
            {
                return TimeToKeepRefundButtonOn = false;
            }
            return true;
        }
        
        public string picUrl()
        {
            var info = imageUrl.FirstOrDefault(x=>x.IsMain);
            return info.PhotoUrl;
        }
    }
}