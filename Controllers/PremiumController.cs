using Microsoft.AspNetCore.Mvc;

namespace IndieArtMarketplace.Controllers
{
    public class PremiumController : Controller
    {
        // Action method for the Premium page
        public IActionResult Index()
        {
            ViewData["Title"] = "Premium";
            return View();
        }
    }
} 