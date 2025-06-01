using System;

namespace IndieArtMarketplace.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } = "Buyer"; // Default role is Buyer
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
    }
}