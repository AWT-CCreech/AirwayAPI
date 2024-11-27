using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.TrakkerControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TrakkerController(eHelpDeskContext context) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;

        [HttpGet("Companies")]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _context.TrkCompanies
                .Where(c => c.CompanyId != "MNS")
                .OrderBy(c => c.CompanyId)
                .Select(c => new { c.CompanyId, c.CompanyName })
                .ToListAsync();

            return Ok(companies);
        }

    }
}
