

namespace Models
{
    [Table("Pictures")]
    public class Photo
    {
        public int Id { get; set; } 
        public bool IsMain { get; set; }
        public string PhotoUrl { get; set; }
        public string PublicId { get; set; }
        public Products products { get; set; }
    }
}