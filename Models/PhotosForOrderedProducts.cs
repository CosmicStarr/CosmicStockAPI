using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    [Table("PicturesForOrderedProducts")]
    public class PhotosForOrderedProducts
    {
        public int Id { get; set; } 
        public bool IsMain { get; set; }
        public string PhotoUrl { get; set; }
        public string PublicId { get; set; }
    }
}