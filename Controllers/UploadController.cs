using Microsoft.AspNetCore.Mvc;
using IndieArtMarketplace.Business.Services;
using IndieArtMarketplace.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using IndieArtMarketplace.DAL;
using System;
using System.Threading.Tasks;

namespace IndieArtMarketplace.Controllers
{
    public class UploadController : Controller
    {
        private readonly UserService _userService;
        private readonly AppDbContext _context;

        public UploadController(UserService userService, AppDbContext context)
        {
            _userService = userService;
            _context = context;
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
        public async Task<IActionResult> UploadArtwork(ArtworkUploadViewModel viewModel)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login", "User");

            if (ModelState.IsValid && viewModel.Title != null && viewModel.Description != null && viewModel.License != null)
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

                // Log the artwork upload
                var uploadLog = new UploadLog
                {
                    UserId = userId.Value,
                    ContentType = "art",
                    UploadedAt = DateTime.UtcNow, // Use UTC time
                    Title = artwork.Title,
                    Price = artwork.Price
                };
                _context.UploadLogs.Add(uploadLog);
                await _context.SaveChangesAsync();

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
        public async Task<IActionResult> UploadMusic(MusicTrackUploadViewModel viewModel)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login", "User");

            if (ModelState.IsValid && viewModel.Title != null && viewModel.Description != null && viewModel.License != null)
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

                // Log the music track upload
                var uploadLog = new UploadLog
                {
                    UserId = userId.Value,
                    ContentType = "music",
                    UploadedAt = DateTime.UtcNow, // Use UTC time
                    Title = musicTrack.Title,
                    Price = musicTrack.Price
                };
                _context.UploadLogs.Add(uploadLog);
                await _context.SaveChangesAsync();

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