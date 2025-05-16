using AirwayAPI.Application;
using AirwayAPI.Models.LoginModels;
using AirwayAPI.Models.SecurityModels;
using AirwayAPI.Services.Interfaces;

namespace AirwayAPI.Services
{
    public class AuthenticationService(ILoginService loginService, ITokenService tokenService) : IAuthenticationService
    {
        private readonly ILoginService _loginService = loginService;
        private readonly ITokenService _tokenService = tokenService;

        public Task<LoginResponse> LoginAsync(LoginRequest login)
            => _loginService.AuthenticateAsync(login);

        public async Task<TokenInfo> RefreshAsync(TokenRefreshRequest req)
        {
            if (!await _tokenService.ValidateRefreshTokenAsync(req.RefreshToken))
                throw new UnauthorizedException("Invalid refresh token.");

            var principal = _tokenService.GetPrincipalFromExpiredToken(req.Token);
            var user = principal.Identity?.Name ?? throw new UnauthorizedException("Invalid JWT.");

            var newJwt = _tokenService.GenerateJwtToken(user);
            var newRt = await _tokenService.GenerateRefreshTokenAsync(user);
            await _tokenService.RevokeRefreshTokenAsync(req.RefreshToken);

            return new TokenInfo { Token = newJwt, RefreshToken = newRt };
        }

        public Task LogoutAsync(string refreshToken)
            => _tokenService.RevokeRefreshTokenAsync(refreshToken);
    }
}