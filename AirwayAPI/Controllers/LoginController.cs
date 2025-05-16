using AirwayAPI.Application;
using AirwayAPI.Models.LoginModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AirwayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController(IAuthenticationService auth, ILogger<LoginController> logger) : ControllerBase
    {
        private readonly IAuthenticationService _auth = auth;
        private readonly ILogger<LoginController> _logger = logger;

        [HttpPost]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req)
        {
            try
            {
                var resp = await _auth.LoginAsync(req);
                return Ok(resp);
            }
            catch (BadRequestException ex) { return BadRequest(new { message = ex.Message }); }
            catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for {Username}", req.Username);
                return StatusCode(500, new { message = "Internal error." });
            }
        }
    }
}