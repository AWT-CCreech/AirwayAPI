﻿using AirwayAPI.Data;
using AirwayAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MassMailerUsersController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public MassMailerUsersController(eHelpDeskContext context)
        {
            _context = context;
        }

        // GET: api/MassMailerUsers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MassMailerUser>>> GetUsers()
        {
            return await _context.Users
                                .Where(u => u.Active == 1)
                                .Select(u => new MassMailerUser
                                {
                                    FullName = u.Fname + " " + u.Lname,
                                    Email = (u.Email ?? string.Empty).ToLower()
                                })
                                .ToListAsync();
        }

    }
}
