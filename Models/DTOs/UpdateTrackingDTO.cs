using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class UpdateTrackingDTO
    {
        public int Id { get; set; }
        public string TrackingNumber { get; set; }
        public DateTimeOffset OrderReceivedInfo { get; set; }
    }
}