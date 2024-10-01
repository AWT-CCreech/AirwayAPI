﻿using AirwayAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers.DataControllers.Sales
{
    [Authorize]
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
                              where d.Id == 2
                                    && u.ActiveSales == 1
                                    && u.Email != null && u.Email.Length > 1
                                    && (u.Uname == "JHerbst" || u.Uname != null && !u.Uname.Contains("house"))
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
                               where t.Ltype == "SalesTeam2" && t.Litem != "All"
                               select new
                               {
                                   t.Id,
                                   t.Litem
                               }).ToListAsync();
            return Ok(teams);
        }

        [HttpGet("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await (from c in _context.SimpleLists
                                    orderby c.Ltag ascending
                                    where c.Ltype == "Category"
                                    select new
                                    {
                                        c.Id,
                                        c.Litem
                                    }).ToListAsync();
            return Ok(categories);
        }

        [HttpGet("GetAccountNumbers")]
        public async Task<IActionResult> GetAccountNumbers()
        {
            var sortedAccounts = await _context.OpenSoreports
                                                .Select(a => new { a.AccountNo })
                                                .Distinct()
                                                .OrderBy(a => a.AccountNo) // Sorting is done in the database
                                                .ToListAsync();

            return Ok(sortedAccounts);
        }
    }
}