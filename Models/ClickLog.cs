using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndieArtMarketplace.Models
{
    public class ClickLog
    {
        public int Id { get; set; }
        public string ButtonType { get; set; }
        public DateTime ClickedAt { get; set; }
        public int UserId { get; set; }
        public string SessionId { get; set; }
    }
} 