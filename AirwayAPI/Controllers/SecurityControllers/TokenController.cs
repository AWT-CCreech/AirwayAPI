using AirwayAPI.Models.SecurityModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AirwayAPI.Controllers.SecurityControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger<TokenController> _logger;

        public TokenController(
            ITokenService tokenService,
            ILogger<TokenController> logger)
        {
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> Refresh([FromBody] TokenRefreshRequest req)
        {
            _logger.LogInformation("Received RefreshToken request for refresh token: {RefreshToken}", req.RefreshToken);

            if (!await _tokenService.ValidateRefreshTokenAsync(req.RefreshToken))
            {
                _logger.LogWarning("RefreshToken validation failed for token: {RefreshToken}", req.RefreshToken);
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }

            var principal = _tokenService.GetPrincipalFromExpiredToken(req.Token);
            var username = principal.Identity?.Name ?? "<unknown>";
            _logger.LogInformation("Principal extracted from expired JWT; username = {Username}", username);

            var newAccess = _tokenService.GenerateJwtToken(username);
            var newRefresh = await _tokenService.GenerateRefreshTokenAsync(username);
            await _tokenService.RevokeRefreshTokenAsync(req.RefreshToken);

            _logger.LogInformation(
                "RefreshToken cycle completed for user {Username}. New access and refresh tokens issued.",
                username
            );

            return Ok(new { Token = newAccess, RefreshToken = newRefresh });
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            _logger.LogInformation("Logout requested; revoking refresh token: {RefreshToken}", refreshToken);

            await _tokenService.RevokeRefreshTokenAsync(refreshToken);

            _logger.LogInformation("Refresh token revoked successfully during logout.");
            return NoContent();
        }
    }
}