using System;

namespace IndieArtMarketplace.Models
{
    public class Transaction
    {
        public int TransactionID { get; set; }
        public int BuyerID { get; set; } // Foreign key to User
        public int? ArtworkID { get; set; } // Nullable (if buying music)
        public int? TrackID { get; set; } // Nullable (if buying artwork)
        public decimal Amount { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Added Status property with default value
    }
}