using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace IndieArtMarketplace.Models
{
    public class MusicTrackUploadViewModel
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public required IFormFile File { get; set; }
        public required string License { get; set; }

        public List<string> AvailableLicenses { get; set; } = new List<string>
        {
            "All Rights Reserved",
            "Creative Commons Attribution",
            "Creative Commons Attribution-ShareAlike",
            "Creative Commons Attribution-NoDerivatives",
            "Creative Commons Attribution-NonCommercial",
            "Creative Commons Zero (Public Domain)"
        };
    }
} 