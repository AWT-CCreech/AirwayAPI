using AirwayAPI.Models;
using AirwayAPI.Models.MassMailerModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirwayAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        // GET: api/Users/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<User>>> GetActiveUsers()
        {
            var users = await _userService.GetActiveUsersAsync();
            return Ok(users);
        }

        // GET: api/Users/massmailer
        [HttpGet("massmailer")]
        public async Task<ActionResult<IEnumerable<MassMailerUser>>> GetMassMailerUsers()
        {
            var users = await _userService.GetMassMailerUsersAsync();
            return Ok(users);
        }
    }
}
