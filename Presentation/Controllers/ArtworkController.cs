using IndieArtMarketplace.DAL;
using IndieArtMarketplace.Models;
using Microsoft.AspNetCore.Mvc;

namespace IndieArtMarketplace.Presentation.Controllers
{
    public class ArtworkController : Controller
    {
        private readonly AppDbContext _db;

        public ArtworkController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var artworks = _db.Artworks.ToList();
            return View(artworks); // Will show all artworks
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(); // Will show the "Add Artwork" form
        }

        [HttpPost]
        public IActionResult Create(Artwork artwork)
        {
            _db.Artworks.Add(artwork);
            _db.SaveChanges();
            return RedirectToAction("Index"); // Refresh the list
        }
    }
}