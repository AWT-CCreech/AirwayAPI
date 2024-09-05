using AirwayAPI.Models.SecurityModels;
using AirwayAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace AirwayAPI.Controllers.SecurityControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly ILogger<TokenController> _logger; // Add logger

        public TokenController(TokenService tokenService, ILogger<TokenController> logger) // Inject logger
        {
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("RefreshToken")]
        public IActionResult RefreshToken([FromBody] TokenRefreshRequest tokenRefreshRequest)
        {
            _logger.LogInformation("Attempting to validate the provided token: {Token}", tokenRefreshRequest.Token);

            if (string.IsNullOrWhiteSpace(tokenRefreshRequest.Token))
            {
                _logger.LogWarning("Refresh token request received with an empty or null token.");
                return BadRequest(new { message = "Invalid token." });
            }

            // Attempt to extract the principal from the token (regardless of its expiration)
            ClaimsPrincipal principal;
            try
            {
                principal = _tokenService.GetPrincipalFromExpiredToken(tokenRefreshRequest.Token);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning("Security token exception occurred while validating token: {Message}", ex.Message);
                return BadRequest(new { message = "Invalid or expired token." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while validating the token.");
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }

            if (principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Invalid token provided. The token does not contain a valid user identity.");
                return BadRequest(new { message = "Invalid token or token does not contain a valid user identity." });
            }

            var username = principal.Identity.Name;
            if (string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("Token does not contain a valid username claim.");
                return BadRequest(new { message = "Invalid token or token does not contain a valid username." });
            }

            _logger.LogInformation("Token validated successfully. Generating a new token for user: {UserName}", username);

            // Generate new JWT token
            string newToken;
            try
            {
                newToken = _tokenService.GenerateJwtToken(username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate a new token for user: {UserName}", username);
                return StatusCode(500, new { message = "Failed to generate a new token." });
            }

            _logger.LogInformation("New token generated successfully for user: {UserName}", username);

            return Ok(new { token = newToken });
        }
    }
}
