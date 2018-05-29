using System.ComponentModel.DataAnnotations;

namespace paypart_api_gateway.Models
{
    public class Login
    {
        [Required]
        public string email { get; set; }
        [Required]
        public string password { get; set; }
        public int id { get; set; }
    }
}
