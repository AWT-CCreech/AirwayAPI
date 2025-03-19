using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.PortalModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.UtilityControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PortalUsageController(eHelpDeskContext context, ILogger<PortalUsageController> logger) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;
        private readonly ILogger<PortalUsageController> _logger = logger;

        [HttpPost("LogUsage")]
        public async Task<IActionResult> LogUsage([FromBody] LogPortalUsageRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Url) || string.IsNullOrWhiteSpace(request.Username))
            {
                _logger.LogWarning("Invalid log usage request: URL or Username is missing.");
                return BadRequest("URL and Username are required.");
            }

            var portalMenu = await _context.PortalMenus.FirstOrDefaultAsync(p => p.Link == request.Url);
            if (portalMenu == null)
            {
                _logger.LogWarning("Portal menu item not found for URL: {Url}", request.Url);
                return NotFound("Portal menu item not found.");
            }

            var today = DateTime.Today;
            var usage = await _context.TrkUsages
                                      .FirstOrDefaultAsync(u => u.AppId == portalMenu.Id && u.Uname == request.Username && u.EntryDate >= today && u.EntryDate < today.AddDays(1));

            if (usage == null)
            {
                var newUsage = new TrkUsage
                {
                    AppId = portalMenu.Id,
                    Uname = request.Username,
                    Ipaddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    EntryDate = DateTime.Now
                };

                _context.TrkUsages.Add(newUsage);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New usage logged for user: {Username}, URL: {Url}", request.Username, request.Url);
                return Ok(new { message = "Usage logged.", usageId = newUsage.RowId });
            }

            _logger.LogInformation("Usage already exists for user: {Username}, URL: {Url} today.", request.Username, request.Url);
            return Ok("Usage already logged today.");
        }

    }
}
