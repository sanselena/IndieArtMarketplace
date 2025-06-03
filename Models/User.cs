using System;

namespace IndieArtMarketplace.Models
{
    public class User
    {
        public int UserID { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string Role { get; set; } = "Buyer"; // Default role is Buyer
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
    }
}