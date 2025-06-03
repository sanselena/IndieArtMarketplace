using System;

namespace IndieArtMarketplace.Models
{
    public class Artwork
    {
        public int ArtworkID { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public required string FileURL { get; set; } // Path to image file
        public DateTime UploadDate { get; set; } = DateTime.Now;
        public int ArtistID { get; set; } // Foreign key to User
        public required string License { get; set; } // License type for the artwork
    }
}