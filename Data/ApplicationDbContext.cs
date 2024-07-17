namespace Data
{
    public class ApplicationDbContext:IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {
            
        }

        public DbSet<Products> GetProducts { get; set; }
        public DbSet<ProductDetails> GetProductDetails { get; set; }
        public DbSet<ProductRating> GetProductRatings { get; set; }
        public DbSet<AppUser> GetAppUsers { get; set; }
        public DbSet<UserAddress> GetUserAddresses { get; set; }
        public DbSet<State> GetStates { get; set; }
        public DbSet<Brand> GetBrands { get; set; }
        public DbSet<Category> GetCategories { get; set; }
        public DbSet<ActualOrder> GetActualOrders { get; set; }
        public DbSet<OrderedProducts> GetOrderedProducts { get; set; }
        public DbSet<WishedProducts> GetWishedProducts { get; set; }
        public DbSet<ShoppingCartSessionId> GetShoppingCartSessionIds { get; set; }
        public DbSet<UserAddressToSaveOrNot> GetUserAddressToSaveOrNots { get; set; }
        public DbSet<RefundModel> GetRefundModels { get; set; }
        
    }
}