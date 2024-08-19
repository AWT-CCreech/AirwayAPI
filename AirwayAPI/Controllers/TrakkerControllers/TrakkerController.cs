using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TrakkerController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public TrakkerController(eHelpDeskContext context)
        {
            _context = context;
        }

        
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
