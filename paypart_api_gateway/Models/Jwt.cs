using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace paypart_api_gateway.Models
{
    public class Jwt
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
    }
}
