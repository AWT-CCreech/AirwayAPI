using System.Security.Claims;

namespace AirwayAPI.Services
{
    public interface ITokenService
    {
        string GenerateJwtToken(string username);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}