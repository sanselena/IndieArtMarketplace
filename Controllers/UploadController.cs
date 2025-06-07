using Microsoft.AspNetCore.Mvc;
using IndieArtMarketplace.Business.Services;
using IndieArtMarketplace.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using IndieArtMarketplace.DAL;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace IndieArtMarketplace.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        private readonly UserService _userService;
        private readonly AppDbContext _context;
        private readonly ILogger<UploadController> _logger;
        private const int MaxFileSizeInMB = 10; // 10MB limit
        private const int MaxFileSize = 10 * 1024 * 1024; // 10MB

        public UploadController(UserService userService, AppDbContext context, ILogger<UploadController> logger)
        {
            _userService = userService;
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            // No need to check HttpContext.Session.GetInt32("UserID") here,
            // as the [Authorize] attribute ensures the user is authenticated.
            // If not authenticated, they would be redirected by the middleware.
            var viewModel = new ArtworkUploadViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UploadArtwork(ArtworkUploadViewModel viewModel)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    _logger.LogWarning("User ID not found in claims or invalid format for UploadArtwork.");
                    return Unauthorized(); // Or RedirectToAction("Login", "User");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for artwork upload");
                    return View("Index", viewModel);
                }

                if (viewModel.File == null || viewModel.File.Length == 0)
                {
                    ModelState.AddModelError("File", "Please select a file to upload");
                    return View("Index", viewModel);
                }

                if (viewModel.File.Length > MaxFileSize)
                {
                    ModelState.AddModelError("File", $"File size exceeds the maximum limit of {MaxFileSize / (1024 * 1024)}MB");
                    return View("Index", viewModel);
                }

                string fileUrl = "/uploads/default.jpg"; // Default value
                if (viewModel.File != null && viewModel.File.Length > 0)
                {
                    try
                    {
                        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                        if (!Directory.Exists(uploads))
                        {
                            Directory.CreateDirectory(uploads);
                            _logger.LogInformation("Created uploads directory at {Path}", uploads);
                        }

                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(viewModel.File.FileName)}";
                        var filePath = Path.Combine(uploads, fileName);
                        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await viewModel.File.CopyToAsync(stream);
                        }
                        
                        fileUrl = "/uploads/" + fileName;
                        _logger.LogInformation("Successfully saved file to {Path}", filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving file");
                        ModelState.AddModelError("File", "Error saving file. Please try again.");
                        return View("Index", viewModel);
                    }
                }

                var artwork = new Artwork
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    Price = viewModel.Price,
                    License = viewModel.License,
                    ArtistID = userId,
                    FileURL = fileUrl,
                    UploadDate = DateTime.UtcNow // Ensure UTC time
                };

                try
                {
                    await _userService.CreateArtwork(artwork); // Make sure this is awaited if it's async

                    // Log the artwork upload
                    var uploadLog = new UploadLog
                    {
                        UserId = userId,
                        ContentType = "art",
                        UploadedAt = DateTime.UtcNow,
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

                    _logger.LogInformation("Successfully uploaded artwork {Title} by user {UserId}", artwork.Title, userId);
                    return RedirectToAction("Success", new { type = "artwork" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving artwork to database");
                    ModelState.AddModelError("", "Error saving artwork. Please try again.");
                    return View("Index", viewModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during artwork upload");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View("Index", viewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadMusic(MusicTrackUploadViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state during music track upload");
                    return View("Index", viewModel);
                }

                if (viewModel.File == null || viewModel.File.Length == 0)
                {
                    ModelState.AddModelError("File", "Please select a file to upload");
                    return View("Index", viewModel);
                }

                if (viewModel.File.Length > MaxFileSize)
                {
                    ModelState.AddModelError("File", $"File size exceeds the maximum limit of {MaxFileSize / (1024 * 1024)}MB");
                    return View("Index", viewModel);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier); // Retrieve from claims
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    _logger.LogWarning("User ID not found in claims or invalid format for UploadMusic.");
                    return Unauthorized();
                }

                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                
                if (!Directory.Exists(uploadsDir))
                {
                    _logger.LogInformation("Creating uploads directory at {Path}", uploadsDir);
                    Directory.CreateDirectory(uploadsDir);
                }

                var fileName = $"{Guid.NewGuid()}_{viewModel.File.FileName}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.File.CopyToAsync(stream);
                }

                _logger.LogInformation("File saved successfully at {Path}", filePath);

                var fileUrl = $"/uploads/{fileName}";
                var musicTrack = new MusicTrack
                {
                    ArtistID = userId,
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    FileURL = fileUrl,
                    Price = viewModel.Price,
                    License = viewModel.License,
                    UploadDate = DateTime.UtcNow // Ensure UTC time
                };

                try
                {
                    await _userService.CreateMusicTrack(musicTrack);

                    // Log the music track upload
                    var uploadLog = new UploadLog
                    {
                        UserId = userId,
                        ContentType = "music",
                        UploadedAt = DateTime.UtcNow,
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

                    _logger.LogInformation("Successfully uploaded music track {Title} by user {UserId}", musicTrack.Title, userId);
                    return RedirectToAction("Success", new { type = "music" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving music track to database");
                    ModelState.AddModelError("", "Error saving music track. Please try again.");
                    return View("Index", viewModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during music track upload");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View("Index", viewModel);
            }
        }

        public IActionResult Success(string type)
        {
            ViewBag.Type = type;
            return View();
        }
    }
} 