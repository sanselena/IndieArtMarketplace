using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndieArtMarketplace.Models
{
    public class UploadLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ContentType { get; set; }
        public DateTime UploadedAt { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
    }
} 