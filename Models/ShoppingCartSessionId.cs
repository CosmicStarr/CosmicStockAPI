using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class ShoppingCartSessionId
    {
        public int Id { get; set; }
        public string ActualShoppingCartId { get; set; }
        public string ApplicationUser { get; set; }
        public DateTime TimeToStayAlive { get; set; } = DateTime.Now;
        public TimeSpan GetTime()
        {
            var timeInfo =  DateTime.Now - TimeToStayAlive;
            return timeInfo;
        }
    }
}