using AirwayAPI.Models.LoginModels;

namespace AirwayAPI.Services.Interfaces;

public interface ILoginService
{
    /// <summary>
    /// Validates credentials, authenticates via SMTP, issues tokens.
    /// Throws BadRequest/NotFound/Unauthorized on failure.
    /// </summary>
    Task<LoginResponse> AuthenticateAsync(LoginRequest login);
}