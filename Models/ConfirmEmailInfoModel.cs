using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class ConfirmEmailInfoModel
    {
        public string Token { get; set; }
        public string UserId { get; set; }
    }
}