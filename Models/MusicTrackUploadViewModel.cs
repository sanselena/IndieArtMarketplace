using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace IndieArtMarketplace.Models
{
    public class MusicTrackUploadViewModel
    {
        public MusicTrackUploadViewModel()
        {
            AvailableLicenses = new List<string>
            {
                "All Rights Reserved",
                "Creative Commons Attribution",
                "Creative Commons Attribution-ShareAlike",
                "Creative Commons Attribution-NoDerivatives",
                "Creative Commons Attribution-NonCommercial",
                "Creative Commons Zero (Public Domain)"
            };
        }

        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public IFormFile? File { get; set; }
        public string? License { get; set; }

        public List<string> AvailableLicenses { get; set; }
    }
} 