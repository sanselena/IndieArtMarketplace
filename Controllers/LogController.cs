using Microsoft.AspNetCore.Mvc;
using IndieArtMarketplace.DAL;
using IndieArtMarketplace.Models;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace IndieArtMarketplace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LogController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("click")]
        public async Task<IActionResult> LogClick([FromBody] ClickLogData clickLogData)
        {
            if (clickLogData == null || string.IsNullOrEmpty(clickLogData.ButtonType))
            {
                return BadRequest("ButtonType is required.");
            }

            int? userId = null;
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                {
                    userId = parsedUserId;
                }
            }

            string? sessionId = HttpContext.Session?.Id;

            var clickLog = new ClickLog
            {
                ButtonType = clickLogData.ButtonType,
                ClickedAt = DateTime.UtcNow, // Use UTC time
                UserId = userId,
                SessionId = sessionId
            };

            _context.ClickLogs.Add(clickLog);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }

    // Helper class to receive click log data from the frontend
    public class ClickLogData
    {
        public string ButtonType { get; set; }
        // UserId and SessionId are captured on the backend
    }
} 