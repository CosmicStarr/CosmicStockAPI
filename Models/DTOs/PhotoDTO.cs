using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class PhotoDTO
    {
        public bool IsMain { get; set; }
        public string PhotoUrl { get; set; }
        public string PublicId { get; set; }
    }
}