using System;

namespace IndieArtMarketplace.Models
{
    public class MusicTrack
    {
        public int TrackID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string FileURL { get; set; } // Path to audio file
        public DateTime UploadDate { get; set; } = DateTime.Now;
        public int ArtistID { get; set; } // Foreign key to User
        public string License { get; set; } // License type for the music track
    }
}