using Microsoft.AspNetCore.Mvc;
using IndieArtMarketplace.Business.Services;
using IndieArtMarketplace.Models;
using System.Linq;
using System;

namespace IndieArtMarketplace.Controllers
{
    public class BrowseController : Controller
    {
        private readonly UserService _userService;

        public BrowseController(UserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index(string type = null, string searchTerm = null)
        {
            var artworks = _userService.GetAllArtworks().ToList();
            var musicTracks = _userService.GetAllMusicTracks().ToList();
            var users = _userService.GetAllUsers().ToList(); // Load all users once
            var userDict = users.ToDictionary(u => u.UserID, u => u.Username); // For fast lookup

            var products = new List<(object Product, string ArtistName)>();
            foreach (var art in artworks)
            {
                userDict.TryGetValue(art.ArtistID, out var artistName);
                products.Add((art, artistName ?? "Unknown"));
            }
            foreach (var music in musicTracks)
            {
                userDict.TryGetValue(music.ArtistID, out var artistName);
                products.Add((music, artistName ?? "Unknown"));
            }

            // Randomize the order of products
            products = products.OrderBy(p => Guid.NewGuid()).ToList();

            if (!string.IsNullOrEmpty(type))
            {
                if (type.ToLower() == "art")
                    return View("Index", products.Where(p => p.Product is Artwork));
                if (type.ToLower() == "music")
                    return View("Index", products.Where(p => p.Product is MusicTrack));
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => (p.Product is Artwork a && a.Title.Contains(searchTerm, System.StringComparison.OrdinalIgnoreCase)) ||
                                                 (p.Product is MusicTrack m && m.Title.Contains(searchTerm, System.StringComparison.OrdinalIgnoreCase))).ToList();
            }

            return View("Index", products);
        }

        public IActionResult Details(int id, string type)
        {
            if (type.ToLower() == "art")
            {
                var artwork = _userService.GetArtworkById(id);
                var artist = _userService.GetAllUsers().FirstOrDefault(u => u.UserID == artwork.ArtistID);
                ViewBag.ArtistName = artist?.Username ?? "Unknown";
                return View("Details", artwork);
            }
            else
            {
                var musicTrack = _userService.GetMusicTrackById(id);
                var artist = _userService.GetAllUsers().FirstOrDefault(u => u.UserID == musicTrack.ArtistID);
                ViewBag.ArtistName = artist?.Username ?? "Unknown";
                return View("Details", musicTrack);
            }
        }

        [HttpGet]
        public IActionResult Purchase(int id, string type)
        {
            // Placeholder for getting the current user ID from session
            // You'll need to implement user authentication and session management
            var userId = HttpContext.Session.GetInt32("UserID"); // Assuming UserID is stored in session
            if (userId == null)
            {
                // Redirect to login if user is not logged in
                return RedirectToAction("Login", "User");
            }

            decimal itemPrice = 0;
            int? artworkId = null;
            int? trackId = null;

            if (type.ToLower() == "art")
            {
                var artwork = _userService.GetArtworkById(id);
                if (artwork == null) return NotFound(); // Item not found
                itemPrice = artwork.Price;
                artworkId = artwork.ArtworkID;
            }
            else if (type.ToLower() == "music")
            {
                var musicTrack = _userService.GetMusicTrackById(id);
                if (musicTrack == null) return NotFound(); // Item not found
                itemPrice = musicTrack.Price;
                trackId = musicTrack.TrackID;
            }
            else
            {
                return BadRequest("Invalid item type."); // Invalid type provided
            }

            var transaction = new Transaction
            {
                BuyerID = userId.Value,
                ArtworkID = artworkId,
                TrackID = trackId,
                Amount = itemPrice,
                PurchaseDate = DateTime.Now // Set purchase date
            };

            _userService.CreateTransaction(transaction);

            // Redirect to a success page
            return RedirectToAction("PurchaseSuccess");
        }

        public IActionResult PurchaseSuccess()
        {
            return View();
        }
    }
} 