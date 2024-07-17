using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Util
{
    public class TokenSettings
    {
        public string SecretKey { get; set; }
        public string VaildIssuer { get; set; }
        public string VaildAudience { get; set; }
    }
}