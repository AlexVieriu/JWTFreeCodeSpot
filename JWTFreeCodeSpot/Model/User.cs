using System;

namespace JWTFreeCodeSpot.Model
{
    public class User
    {
        public int IdUser { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime Date { get; set; }
    }
}
