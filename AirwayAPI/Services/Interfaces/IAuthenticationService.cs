using AirwayAPI.Models.LoginModels;
using AirwayAPI.Models.SecurityModels;

namespace AirwayAPI.Services.Interfaces;

public interface IAuthenticationService
{
    Task<LoginResponse> LoginAsync(LoginRequest login);
    Task<TokenInfo> RefreshAsync(TokenRefreshRequest req);
    Task LogoutAsync(string refreshToken);
}