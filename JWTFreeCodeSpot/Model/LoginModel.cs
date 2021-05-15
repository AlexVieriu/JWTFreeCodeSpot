﻿using System.ComponentModel.DataAnnotations;

namespace JWTFreeCodeSpot.Model
{
    public class LoginModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}