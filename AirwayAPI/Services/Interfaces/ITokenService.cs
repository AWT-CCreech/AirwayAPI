using System.Security.Claims;

namespace AirwayAPI.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(string username);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}