using AirwayAPI.Application;
using AirwayAPI.Data;
using AirwayAPI.Models.LoginModels;
using AirwayAPI.Models.SecurityModels;
using AirwayAPI.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Services
{
    public class LoginService(
        eHelpDeskContext context,
        ITokenService tokenService,
        IUserService userService,
        ILogger<LoginService> logger) : ILoginService
    {
        private readonly eHelpDeskContext _context = context;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IUserService _userService = userService;
        private readonly ILogger<LoginService> _logger = logger;

        private static readonly string[] PredefinedUsers =
            ["mgibson", "lvonder", "kgildersleeve"];

        public async Task<LoginResponse> AuthenticateAsync(LoginRequest login)
        {
            if (string.IsNullOrWhiteSpace(login.Username) ||
                string.IsNullOrWhiteSpace(login.Password))
                throw new BadRequestException("Username or password cannot be empty.");

            var normalized = login.Username.Trim().ToLower();
            var exists = await _context.Users.AnyAsync(u =>
                (u.Uname ?? string.Empty).Trim().ToLower() == normalized)
                || PredefinedUsers.Contains(normalized);

            if (!exists)
                throw new NotFoundException("User not found.");

            if (login.IsPasswordEncrypted)
            {
                if (!IsBase64String(login.Password))
                    throw new BadRequestException("Invalid Base64 password.");
                login.Password = LoginUtils.DecryptPassword(login.Password);
            }

            var authResult = await AuthenticateSmtpAsync(login.Username, login.Password);
            if (!authResult.IsSuccess)
                throw new UnauthorizedException("Authentication failed.");

            var userId = await _userService.GetUserIdAsync(normalized);
            var accessToken = _tokenService.GenerateJwtToken(login.Username);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(login.Username);

            return new LoginResponse
            {
                Userid = userId.ToString(),
                Username = login.Username,
                Token = accessToken,
                RefreshToken = refreshToken
            };
        }

        private static bool IsBase64String(string s)
        {
            Span<byte> buffer = new byte[s.Length];
            return Convert.TryFromBase64String(s, buffer, out _);
        }

        private async Task<AuthenticationResult> AuthenticateSmtpAsync(string username, string password)
        {
            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync($"{username}@airway.com", password);
                await client.DisconnectAsync(true);
                return AuthenticationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP auth failed for {Username}", username);
                return AuthenticationResult.Failure();
            }
        }
    }
}