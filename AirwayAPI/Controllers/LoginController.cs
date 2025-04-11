using AirwayAPI.Application;
using AirwayAPI.Data;
using AirwayAPI.Models.LoginModels;
using AirwayAPI.Models.SecurityModels;
using AirwayAPI.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AirwayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController(eHelpDeskContext context, ILogger<LoginController> logger, ITokenService tokenService, IUserService userService) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;
        private readonly ILogger<LoginController> _logger = logger;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IUserService _userService = userService;
        private static readonly string[] predefinedUserArray = ["mgibson", "lvonder", "kgildersleeve"];

        [HttpPost]
        public async Task<ActionResult<LoginInfo>> LoginUser([FromBody] LoginInfo login)
        {
            if (string.IsNullOrWhiteSpace(login.username) || string.IsNullOrWhiteSpace(login.password))
                return BadRequest(new { message = "Username or password cannot be empty." });

            var usernameTrimmedLower = login.username.Trim().ToLower();
            var exists = await _context.Users.AnyAsync(u => (u.Uname ?? string.Empty).Trim().ToLower() == usernameTrimmedLower) || predefinedUserArray.Contains(usernameTrimmedLower);
            if (!exists)
                return NotFound(new { message = "User not found." });

            try
            {
                if (login.isPasswordEncrypted)
                {
                    if (!IsBase64String(login.password))
                        return BadRequest(new { message = "Invalid Base64 password." });
                    login.password = LoginUtils.DecryptPassword(login.password);
                }

                var authResult = await AuthenticateUserAsync(login.username!, login.password);
                if (!authResult.IsSuccess)
                    return Unauthorized(new { message = "Authentication failed." });

                var userId = await _userService.GetUserIdAsync(usernameTrimmedLower);
                var accessToken = _tokenService.GenerateJwtToken(login.username!);
                var refreshToken = await _tokenService.GenerateRefreshTokenAsync(login.username!);

                login.password = LoginUtils.EncryptPassword(login.password);
                login.isPasswordEncrypted = true;
                login.userid = userId.ToString();
                login.token = accessToken;
                login.refreshToken = refreshToken;

                return Ok(login);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for {Username}", login.username);
                return StatusCode(500, new { message = "Internal error." });
            }
        }

        private async Task<AuthenticationResult> AuthenticateUserAsync(string username, string password)
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
                _logger.LogError(ex, "SMTP authentication failed for {Username}", username);
                return AuthenticationResult.Failure();
            }
        }

        private static bool IsBase64String(string s)
        {
            Span<byte> buffer = new byte[s.Length];
            return Convert.TryFromBase64String(s, buffer, out _);
        }
    }
}