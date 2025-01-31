using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.UtilityControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PurchasingController(eHelpDeskContext context) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;

        /// <summary>
        /// Retrieves a list of active purchasing reps.
        /// </summary>
        /// <returns>A list of active purchasing reps.</returns>
        [HttpGet("GetPurchasingReps")]
        public async Task<IActionResult> GetPurchasingReps()
        {
            var reps = await _context.Users
                                     .Where(u => u.Active == 1 &&
                                                 (u.DeptId == 10 ||
                                                  u.Uname == "LVonderporten" ||
                                                  u.Uname == "JSmith"))
                                     .OrderBy(u => u.Uname)
                                     .Select(u => new
                                     {
                                         u.Id,
                                         u.Lname,
                                         u.Fname,
                                         u.Uname
                                     })
                                     .ToListAsync();

            return Ok(reps);
        }
    }
}
