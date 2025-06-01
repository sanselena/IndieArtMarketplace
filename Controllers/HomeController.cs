using Microsoft.AspNetCore.Mvc;

namespace IndieArtMarketplace.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
} 