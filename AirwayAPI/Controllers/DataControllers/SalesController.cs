using AirwayAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace AirwayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public SalesController(eHelpDeskContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of active sales reps.
        /// </summary>
        /// <returns>A list of active sales reps.</returns>
        [HttpGet("GetSalesReps")]
        public async Task<IActionResult> GetSalesReps()
        {
            var reps = await (from u in _context.Users
                              join d in _context.Departments on u.DeptId equals d.Id
                              where d.Id == 2 && u.ActiveSales == 1 && u.Email.Length > 1 && !u.Uname.Contains("house")
                                    || u.Uname == "JHerbst"
                              orderby u.Uname
                              select new
                              {
                                  u.Id,
                                  u.Lname,
                                  u.Fname,
                                  u.Uname
                              }).ToListAsync();

            return Ok(reps);
        }

        [HttpGet("GetSalesTeams")]
        public async Task<IActionResult> GetSalesTeams()
        {
            var teams = await (from t in _context.SimpleLists
                               orderby t.Litem ascending
                               where t.Ltype == "SalesTeam2"
                               select new
                               {
                                   t.Litem
                               }).ToListAsync();
            return Ok(teams);
        }
    }
}
