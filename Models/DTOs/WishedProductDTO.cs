using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class WishedProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal price { get; set; }
        public string imageUrl { get; set; }
    }
}