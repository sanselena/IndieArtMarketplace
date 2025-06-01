using Microsoft.AspNetCore.Mvc;
using IndieArtMarketplace.Business.Services;
using IndieArtMarketplace.Models;
using Microsoft.AspNetCore.Http;

namespace IndieArtMarketplace.Controllers
{
    public class UserController : Controller
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string loginInput, string password)
        {
            var user = _userService.GetUserByEmailOrUsername(loginInput);
            if (user != null && user.Password == password)
            {
                HttpContext.Session.SetInt32("UserID", user.UserID);
                return RedirectToAction("Profile");
            }
            ViewBag.Error = "Invalid credentials.";
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (_userService.GetUserByEmailOrUsername(user.Email) != null ||
                _userService.GetUserByEmailOrUsername(user.Username) != null)
            {
                ViewBag.Error = "Email or username already registered.";
                return View(user);
            }
            if (ModelState.IsValid)
            {
                _userService.CreateUser(user);
                return RedirectToAction("Login");
            }
            return View(user);
        }

        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login");

            var user = _userService.GetAllUsers().FirstOrDefault(u => u.UserID == userId);
            if (user == null)
                return RedirectToAction("Login");

            // Get user's uploaded artwork
            ViewBag.Artworks = _userService.GetAllArtworks()
                .Where(a => a.ArtistID == userId)
                .ToList();

            // Get user's purchase history
            ViewBag.Transactions = _userService.GetAllTransactions()
                .Where(t => t.BuyerID == userId)
                .OrderByDescending(t => t.PurchaseDate)
                .ToList();

            return View(user);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
} 