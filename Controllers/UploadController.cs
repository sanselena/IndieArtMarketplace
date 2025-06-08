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

namespace IndieArtMarketplace.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        private readonly UserService _userService;
        private readonly AppDbContext _context;
        private readonly ILogger<UploadController> _logger;
        private readonly IBlobStorageService _blobStorageService;
        private const int MaxFileSizeInMB = 10; // 10MB limit
        private const int MaxFileSize = 10 * 1024 * 1024; // 10MB
        private const string ArtworkContainerName = "artwork";
        private const string MusicContainerName = "music";

        public UploadController(UserService userService, AppDbContext context, ILogger<UploadController> logger, IBlobStorageService blobStorageService)
        {
            _userService = userService;
            _context = context;
            _logger = logger;
            _blobStorageService = blobStorageService;
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

                string fileUrl = "";
                if (viewModel.File != null && viewModel.File.Length > 0)
                {
                    try
                    {
                        var fileName = await _blobStorageService.UploadFileAsync(viewModel.File, ArtworkContainerName);
                        fileUrl = _blobStorageService.GetBlobUrl(fileName, ArtworkContainerName);
                        _logger.LogInformation("Successfully uploaded file to Azure Blob Storage. URL: {FileUrl}", fileUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during Azure Blob Storage upload process for artwork.");
                        ModelState.AddModelError("File", "Error uploading file to Azure Blob Storage. Please try again.");
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
                return View("Index", new MusicTrackUploadViewModel());
            }

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    _logger.LogWarning("User ID not found in claims or invalid format for UploadMusic.");
                    return Unauthorized();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for music upload");
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
                    ModelState.AddModelError("File", "Please select a music file to upload.");
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
                    ModelState.AddModelError("File", $"File size exceeds the maximum limit of {MaxFileSize / (1024 * 1024)}MB.");
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

                string fileUrl = "";
                if (viewModel.File != null && viewModel.File.Length > 0)
                {
                    try
                    {
                        var fileName = await _blobStorageService.UploadFileAsync(viewModel.File, MusicContainerName);
                        fileUrl = _blobStorageService.GetBlobUrl(fileName, MusicContainerName);
                        _logger.LogInformation("Successfully uploaded music file to Azure Blob Storage. URL: {FileUrl}", fileUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during Azure Blob Storage upload process for music.");
                        ModelState.AddModelError("File", "Error uploading music file to Azure Blob Storage. Please try again.");
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

                var musicTrack = new MusicTrack
                {
                    Title = viewModel.Title,
                    ArtistID = userId,
                    Description = viewModel.Description,
                    Price = viewModel.Price,
                    License = viewModel.License,
                    FileURL = fileUrl,
                    UploadDate = DateTime.UtcNow
                };

                try
                {
                    await _userService.CreateMusicTrack(musicTrack);

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
                    if (viewModel == null)
                    {
                        viewModel = new MusicTrackUploadViewModel();
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
                _logger.LogError(ex, "Unexpected error during music upload");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                if (viewModel == null)
                {
                    viewModel = new MusicTrackUploadViewModel();
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

        public IActionResult Success(string type)
        {
            return View();
        }
    }
} 