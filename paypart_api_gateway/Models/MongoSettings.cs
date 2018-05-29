using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace paypart_api_gateway.Models
{
    public class MongoSettings
    {
        public string connectionString { get; set; }
        public string database { get; set; }
        public string collection { get; set; }
    }
}
