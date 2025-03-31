using AirwayAPI.Models.SecurityModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AirwayAPI.Controllers.SecurityControllers;

[ApiController]
[Route("api/[controller]")]
public class TokenController(ITokenService tokenService) : ControllerBase
{
    private readonly ITokenService _tokenService = tokenService;

    [HttpPost("RefreshToken")]
    public async Task<IActionResult> Refresh(TokenRefreshRequest req)
    {
        if (!await _tokenService.ValidateRefreshTokenAsync(req.RefreshToken)) return Unauthorized();
        var principal = _tokenService.GetPrincipalFromExpiredToken(req.Token);
        var username = principal.Identity!.Name!;
        var newAccess = _tokenService.GenerateJwtToken(username);
        var newRefresh = await _tokenService.GenerateRefreshTokenAsync(username);
        await _tokenService.RevokeRefreshTokenAsync(req.RefreshToken);
        return Ok(new { Token = newAccess, RefreshToken = newRefresh });
    }

    [HttpPost("Logout")]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        await _tokenService.RevokeRefreshTokenAsync(refreshToken);
        return NoContent();
    }
}