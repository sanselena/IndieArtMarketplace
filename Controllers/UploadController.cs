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
using System.Collections.Generic;
using ImageKit;

namespace IndieArtMarketplace.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        private readonly UserService _userService;
        private readonly AppDbContext _context;
        private readonly ILogger<UploadController> _logger;
        private readonly ImageKitClient _imageKitClient;
        private const int MaxFileSizeInMB = 10; // 10MB limit
        private const int MaxFileSize = 10 * 1024 * 1024; // 10MB

        public UploadController(UserService userService, AppDbContext context, ILogger<UploadController> logger, ImageKitClient imageKitClient)
        {
            _userService = userService;
            _context = context;
            _logger = logger;
            _imageKitClient = imageKitClient;
        }

        public IActionResult Index()
        {
            var viewModel = new ArtworkUploadViewModel();
            // Ensure AvailableLicenses is populated when displaying the initial view
            viewModel.AvailableLicenses = new List<string>
            {
                "All Rights Reserved",
                "Creative Commons Attribution",
                "Creative Commons Attribution-ShareAlike",
                "Creative Commons Attribution-NoDerivatives",
                "Creative Commons Attribution-NonCommercial",
                "Creative Commons Zero (Public Domain)"
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UploadArtwork([FromForm] ArtworkUploadViewModel viewModel)
        {
            _logger.LogInformation("UploadArtwork action entered.");

            // Always ensure viewModel is not null before proceeding
            if (viewModel == null)
            {
                _logger.LogWarning("UploadArtwork: ViewModel is null upon submission - Model Binding Failed.");
                ModelState.AddModelError("", "Invalid upload data received. Please ensure all fields are correctly filled and try again.");
                return View("Index", new ArtworkUploadViewModel()); // Return new, populated ViewModel
            }

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    _logger.LogWarning("User ID not found in claims or invalid format for UploadArtwork.");
                    return Unauthorized();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for artwork upload");
                    // Repopulate AvailableLicenses on validation failure
                    viewModel.AvailableLicenses = new List<string>
                    {
                        "All Rights Reserved",
                        "Creative Commons Attribution",
                        "Creative Commons Attribution-ShareAlike",
                        "Creative Commons Attribution-NoDerivatives",
                        "Creative Commons Attribution-NonCommercial",
                        "Creative Commons Zero (Public Domain)"
                    };
                    return View("Index", viewModel);
                }

                if (viewModel.File == null || viewModel.File.Length == 0)
                {
                    ModelState.AddModelError("File", "Please select a file to upload");
                    // Repopulate AvailableLicenses on validation failure
                    viewModel.AvailableLicenses = new List<string>
                    {
                        "All Rights Reserved",
                        "Creative Commons Attribution",
                        "Creative Commons Attribution-ShareAlike",
                        "Creative Commons Attribution-NoDerivatives",
                        "Creative Commons Attribution-NonCommercial",
                        "Creative Commons Zero (Public Domain)"
                    };
                    return View("Index", viewModel);
                }

                if (viewModel.File.Length > MaxFileSize)
                {
                    ModelState.AddModelError("File", $"File size exceeds the maximum limit of {MaxFileSize / (1024 * 1024)}MB");
                    // Repopulate AvailableLicenses on validation failure
                    viewModel.AvailableLicenses = new List<string>
                    {
                        "All Rights Reserved",
                        "Creative Commons Attribution",
                        "Creative Commons Attribution-ShareAlike",
                        "Creative Commons Attribution-NoDerivatives",
                        "Creative Commons Attribution-NonCommercial",
                        "Creative Commons Zero (Public Domain)"
                    };
                    return View("Index", viewModel);
                }

                string fileUrl = ""; // Initialize with empty string, will be set by ImageKit
                if (viewModel.File != null && viewModel.File.Length > 0)
                {
                    try
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await viewModel.File.CopyToAsync(memoryStream);
                            var fileBytes = memoryStream.ToArray();
                            var fileName = Path.GetFileName(viewModel.File.FileName);

                            _logger.LogInformation("Attempting to upload file to ImageKit: {FileName}", fileName);

                            var result = await _imageKitClient.Upload(fileBytes, fileName);

                            if (result != null && result.success)
                            {
                                fileUrl = result.url; // Use result.url from ImageKit
                                _logger.LogInformation("Successfully uploaded file to ImageKit. URL: {FileUrl}", fileUrl);
                            }
                            else
                            {
                                string errorMessage = result?.error?.message ?? "Unknown error during ImageKit upload.";
                                _logger.LogError("ImageKit upload failed for file {FileName}: {ErrorMessage}", fileName, errorMessage);
                                ModelState.AddModelError("File", $"Error uploading file to ImageKit: {errorMessage}");
                                // Repopulate AvailableLicenses on validation failure
                                viewModel.AvailableLicenses = new List<string>
                                {
                                    "All Rights Reserved",
                                    "Creative Commons Attribution",
                                    "Creative Commons Attribution-ShareAlike",
                                    "Creative Commons Attribution-NoDerivatives",
                                    "Creative Commons Attribution-NonCommercial",
                                    "Creative Commons Zero (Public Domain)"
                                };
                                return View("Index", viewModel);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during ImageKit upload process.");
                        ModelState.AddModelError("File", "Error uploading file. Please try again.");
                        // Repopulate AvailableLicenses on validation failure
                        viewModel.AvailableLicenses = new List<string>
                        {
                            "All Rights Reserved",
                            "Creative Commons Attribution",
                            "Creative Commons Attribution-ShareAlike",
                            "Creative Commons Attribution-NoDerivatives",
                            "Creative Commons Attribution-NonCommercial",
                            "Creative Commons Zero (Public Domain)"
                        };
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
                    await _userService.CreateArtwork(artwork); // Ensure this is awaited

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
                    // Ensure viewModel is not null before populating AvailableLicenses
                    if (viewModel == null)
                    {
                        viewModel = new ArtworkUploadViewModel();
                    }
                    viewModel.AvailableLicenses = new List<string>
                    {
                        "All Rights Reserved",
                        "Creative Commons Attribution",
                        "Creative Commons Attribution-ShareAlike",
                        "Creative Commons Attribution-NoDerivatives",
                        "Creative Commons Attribution-NonCommercial",
                        "Creative Commons Zero (Public Domain)"
                    };
                    return View("Index", viewModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during artwork upload");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                // Ensure viewModel is not null before populating AvailableLicenses
                if (viewModel == null)
                {
                    viewModel = new ArtworkUploadViewModel();
                }
                viewModel.AvailableLicenses = new List<string>
                {
                    "All Rights Reserved",
                    "Creative Commons Attribution",
                    "Creative Commons Attribution-ShareAlike",
                    "Creative Commons Attribution-NoDerivatives",
                    "Creative Commons Attribution-NonCommercial",
                    "Creative Commons Zero (Public Domain)"
                };
            return View("Index", viewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadMusic([FromForm] MusicTrackUploadViewModel viewModel)
        {
            _logger.LogInformation("UploadMusic action entered.");

            if (viewModel == null)
            {
                _logger.LogWarning("UploadMusic: ViewModel is null upon submission - Model Binding Failed.");
                ModelState.AddModelError("", "Invalid upload data received. Please ensure all fields are correctly filled and try again.");
                // When returning to Index from UploadMusic, always provide an ArtworkUploadViewModel
                var artworkViewModel = new ArtworkUploadViewModel();
                artworkViewModel.AvailableLicenses = new List<string>
                {
                    "All Rights Reserved",
                    "Creative Commons Attribution",
                    "Creative Commons Attribution-ShareAlike",
                    "Creative Commons Attribution-NoDerivatives",
                    "Creative Commons Attribution-NonCommercial",
                    "Creative Commons Zero (Public Domain)"
                };
                return View("Index", artworkViewModel);
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state during music track upload");
                    // Create a new ArtworkUploadViewModel to correctly display the view
                    var artworkViewModel = new ArtworkUploadViewModel();
                    // Repopulate AvailableLicenses for the new view model
                    artworkViewModel.AvailableLicenses = new List<string>
                    {
                        "All Rights Reserved",
                        "Creative Commons Attribution",
                        "Creative Commons Attribution-ShareAlike",
                        "Creative Commons Attribution-NoDerivatives",
                        "Creative Commons Attribution-NonCommercial",
                        "Creative Commons Zero (Public Domain)"
                    };
                    // Transfer model state errors to the new view model
                    return View("Index", artworkViewModel);
                }

                if (viewModel.File == null || viewModel.File.Length == 0)
                {
                    ModelState.AddModelError("File", "Please select a file to upload");
                    // Repopulate AvailableLicenses on validation failure
                    var artworkViewModel = new ArtworkUploadViewModel();
                    artworkViewModel.AvailableLicenses = new List<string>
                    {
                        "All Rights Reserved",
                        "Creative Commons Attribution",
                        "Creative Commons Attribution-ShareAlike",
                        "Creative Commons Attribution-NoDerivatives",
                        "Creative Commons Attribution-NonCommercial",
                        "Creative Commons Zero (Public Domain)"
                    };
                    // Transfer model state errors to the new view model
                    return View("Index", artworkViewModel);
                }

                if (viewModel.File.Length > MaxFileSize)
                {
                    ModelState.AddModelError("File", $"File size exceeds the maximum limit of {MaxFileSize / (1024 * 1024)}MB");
                    // Repopulate AvailableLicenses on validation failure
                    var artworkViewModel = new ArtworkUploadViewModel();
                    artworkViewModel.AvailableLicenses = new List<string>
                    {
                        "All Rights Reserved",
                        "Creative Commons Attribution",
                        "Creative Commons Attribution-ShareAlike",
                        "Creative Commons Attribution-NoDerivatives",
                        "Creative Commons Attribution-NonCommercial",
                        "Creative Commons Zero (Public Domain)"
                    };
                    // Transfer model state errors to the new view model
                    return View("Index", artworkViewModel);
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

                // Sanitize filename: convert to lowercase, replace spaces with hyphens, remove invalid chars
                var originalMusicFileName = Path.GetFileNameWithoutExtension(viewModel.File.FileName);
                var musicFileExtension = Path.GetExtension(viewModel.File.FileName);
                var sanitizedMusicFileName = string.Join("-", originalMusicFileName.Split(Path.GetInvalidFileNameChars()))
                                            .Replace(" ", "-")
                                            .ToLowerInvariant();
                var uniqueMusicFileName = $"{Guid.NewGuid()}_{sanitizedMusicFileName}{musicFileExtension}";

                var filePath = Path.Combine(uploadsDir, uniqueMusicFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.File.CopyToAsync(stream);
                }

                _logger.LogInformation("File saved successfully at {Path} with name {FileName}", filePath, uniqueMusicFileName);

                var fileUrl = $"/uploads/{uniqueMusicFileName}";
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
                    // Repopulate AvailableLicenses on validation failure
                    var artworkViewModel = new ArtworkUploadViewModel();
                    artworkViewModel.AvailableLicenses = new List<string>
                    {
                        "All Rights Reserved",
                        "Creative Commons Attribution",
                        "Creative Commons Attribution-ShareAlike",
                        "Creative Commons Attribution-NoDerivatives",
                        "Creative Commons Attribution-NonCommercial",
                        "Creative Commons Zero (Public Domain)"
                    };
                    // Transfer model state errors to the new view model
                    return View("Index", artworkViewModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during music track upload");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                // Repopulate AvailableLicenses on validation failure
                var artworkViewModel = new ArtworkUploadViewModel();
                artworkViewModel.AvailableLicenses = new List<string>
                {
                    "All Rights Reserved",
                    "Creative Commons Attribution",
                    "Creative Commons Attribution-ShareAlike",
                    "Creative Commons Attribution-NoDerivatives",
                    "Creative Commons Attribution-NonCommercial",
                    "Creative Commons Zero (Public Domain)"
                };
                // Transfer model state errors to the new view model
                return View("Index", artworkViewModel);
            }
        }

        public IActionResult Success(string type)
        {
            ViewBag.Type = type;
            return View();
        }
    }
} 