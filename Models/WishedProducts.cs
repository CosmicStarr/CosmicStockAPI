using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class WishedProducts
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal price { get; set; }
        public string imageUrl { get; set; }
        [ForeignKey("AppUserId")]
        public string User { get; set; }
    }
}