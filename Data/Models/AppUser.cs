using System;
using Microsoft.AspNetCore.Identity;

namespace Data.Models
{
    public class AppUser : IdentityUser
    {
        public string DisplayName { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
    }
}