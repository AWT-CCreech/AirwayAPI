using System.Security.Claims;

namespace AirwayAPI.Services.Interfaces;
public interface ITokenService
{
    string GenerateJwtToken(string username);
    Task<string> GenerateRefreshTokenAsync(string username);
    Task<bool> ValidateRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}