using System.ComponentModel.DataAnnotations;

namespace IndieArtMarketplace.Models
{
    public class ArtworkUploadViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        public IFormFile File { get; set; }

        [Required]
        public string License { get; set; }

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