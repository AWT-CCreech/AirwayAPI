using AirwayAPI.Data;
using AirwayAPI.Data;
using Microsoft.AspNetCore.Mvc;

namespace AirwayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortalUsageController : ControllerBase
    {
        private readonly eHelpDeskContext _context;
        private readonly ILogger<PortalUsageController> _logger;

        public PortalUsageController(eHelpDeskContext context, ILogger<PortalUsageController> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        [HttpGet("LogUsage")]
        public async Task<IActionResult> LogUsage(string url, string username)
        {
            var portalMenu = _context.PortalMenus.FirstOrDefault(p => p.Link == url);
            if (portalMenu == null)
            {
                return NotFound("Portal menu item not found.");
            }

            var today = DateTime.Today;
            var usage = _context.TrkUsages.FirstOrDefault(u => u.AppId == portalMenu.Id && u.Uname == username && u.EntryDate >= today && u.EntryDate < today.AddDays(1));

            if (usage == null)
            {
                var newUsage = new TrkUsage
                {
                    AppId = portalMenu.Id,
                    Uname = username,
                    Ipaddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    EntryDate = DateTime.Now
                };

                _context.TrkUsages.Add(newUsage);
                await _context.SaveChangesAsync();
            }

            return Ok("Usage logged.");
        }
        
    }
}
