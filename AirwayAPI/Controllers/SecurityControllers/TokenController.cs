using AirwayAPI.Application;
using AirwayAPI.Models.SecurityModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirwayAPI.Controllers.SecurityControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController(IAuthenticationService auth, ILogger<TokenController> logger) : ControllerBase
    {
        private readonly IAuthenticationService _auth = auth;
        private readonly ILogger<TokenController> _logger = logger;

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<TokenInfo>> Refresh([FromBody] TokenRefreshRequest req)
        {
            try
            {
                var ti = await _auth.RefreshAsync(req);
                return Ok(ti);
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh error");
                return StatusCode(500, new { message = "Internal error." });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            await _auth.LogoutAsync(refreshToken);
            return NoContent();
        }
    }
}