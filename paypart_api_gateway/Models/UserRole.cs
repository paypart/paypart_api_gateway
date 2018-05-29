using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace paypart_api_gateway.Models
{
    public class UserRole
    {
        [Key]
        public int _id { get; set; }
        public string role { get; set; }
        public int status { get; set; }
    }
}
