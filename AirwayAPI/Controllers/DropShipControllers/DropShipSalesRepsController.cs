using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.DropShipControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DropShipSalesRepsController(eHelpDeskContext context) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;

        [HttpGet]
        public async Task<ActionResult<object>> GetAllSalesRep()
        {
            try
            {
                var salesReps = await (from u in _context.Users
                                       join d in _context.Departments on u.DeptId equals d.Id
                                       where (d.Id == 2 && u.ActiveSales == 1
                                       && u.Email != null && u.Email.Trim().Length > 1
                                       && (u.Uname == null || !u.Uname.Contains("house")))
                                       orderby u.Lname
                                       select new
                                       {
                                           FullName = u.Fname + " " + u.Lname,
                                           Email = (u.Email ?? string.Empty).ToLower()
                                       })
                                       .ToListAsync();
                return Ok(salesReps);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message + "    .........    " + ex.StackTrace);
            }
        }
    }
}
