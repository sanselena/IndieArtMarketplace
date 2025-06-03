using Microsoft.AspNetCore.Mvc;
using IndieArtMarketplace.Business.Services;
using IndieArtMarketplace.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace IndieArtMarketplace.Controllers
{
    public class UploadController : Controller
    {
        private readonly UserService _userService;

        public UploadController(UserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login", "User");

            var viewModel = new ArtworkUploadViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult UploadArtwork(ArtworkUploadViewModel viewModel)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login", "User");

            if (ModelState.IsValid)
            {
                string fileUrl = "/uploads/default.jpg"; // Default value
                if (viewModel.File != null && viewModel.File.Length > 0)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploads))
                        Directory.CreateDirectory(uploads);

                    var fileName = Path.GetFileName(viewModel.File.FileName);
                    var filePath = Path.Combine(uploads, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        viewModel.File.CopyTo(stream);
                    }
                    fileUrl = "/uploads/" + fileName;
                }

                var artwork = new Artwork
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    Price = viewModel.Price,
                    License = viewModel.License,
                    ArtistID = userId.Value,
                    FileURL = fileUrl
                };

                _userService.CreateArtwork(artwork);

                // Update user role to Artist if they were a Buyer
                var user = _userService.GetAllUsers().FirstOrDefault(u => u.UserID == userId);
                if (user != null && user.Role == "Buyer")
                {
                    user.Role = "Artist";
                    _userService.UpdateUser(user);
                }

                return RedirectToAction("Success", new { type = "artwork" });
            }

            return View("Index", viewModel);
        }

        [HttpPost]
        public IActionResult UploadMusic(MusicTrackUploadViewModel viewModel)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login", "User");

            if (ModelState.IsValid)
            {
                string fileUrl = "/uploads/default.mp3"; // Default value
                if (viewModel.File != null && viewModel.File.Length > 0)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploads))
                        Directory.CreateDirectory(uploads);

                    var fileName = Path.GetFileName(viewModel.File.FileName);
                    var filePath = Path.Combine(uploads, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        viewModel.File.CopyTo(stream);
                    }
                    fileUrl = "/uploads/" + fileName;
                }

                var musicTrack = new MusicTrack
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    Price = viewModel.Price,
                    License = viewModel.License,
                    ArtistID = userId.Value,
                    FileURL = fileUrl
                };

                _userService.CreateMusicTrack(musicTrack);

                // Update user role to Artist if they were a Buyer
                var user = _userService.GetAllUsers().FirstOrDefault(u => u.UserID == userId);
                if (user != null && user.Role == "Buyer")
                {
                    user.Role = "Artist";
                    _userService.UpdateUser(user);
                }

                return RedirectToAction("Success", new { type = "music" });
            }

            return View("Index", viewModel);
        }

        public IActionResult Success(string type)
        {
            ViewBag.Type = type;
            return View();
        }
    }
} 