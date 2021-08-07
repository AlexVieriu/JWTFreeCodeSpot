using System.ComponentModel.DataAnnotations;

namespace JWTFreeCodeSpot_Original.Models
{
    public class LoginModel
    {
        [Required]
        public string email { get; set; }
        [Required]
        public string password { get; set; }
    }
}
