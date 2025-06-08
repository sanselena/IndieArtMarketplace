using System;

namespace IndieArtMarketplace.Models
{
    public class MusicTrack
    {
        public int TrackID { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public required string FileURL { get; set; } // Path to audio file
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        public int ArtistID { get; set; } // Foreign key to User
        public required string License { get; set; } // License type for the music track
    }
}