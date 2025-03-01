﻿using AirwayAPI.Application;
using AirwayAPI.Data;
using AirwayAPI.Models.LoginModels;
using AirwayAPI.Models.SecurityModels;
using AirwayAPI.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserLoginsController(
        eHelpDeskContext context,
        ILogger<UserLoginsController> logger,
        ITokenService tokenService,
        IUserService userService) : ControllerBase
    {
        private readonly eHelpDeskContext _context = context;
        private readonly ILogger<UserLoginsController> _logger = logger;
        private readonly ITokenService _tokenService = tokenService; // Inject ITokenService
        private readonly IUserService _userService = userService; // Inject IUserService
        private static readonly string[] predefinedUserArray = ["mgibson", "lvonder", "kgildersleeve"];

        [HttpPost]
        public async Task<ActionResult<LoginInfo>> GetUsers([FromBody] LoginInfo login)
        {
            if (string.IsNullOrWhiteSpace(login.username) || string.IsNullOrWhiteSpace(login.password))
            {
                _logger.LogWarning("Invalid login request: username or password is missing.");
                return BadRequest(new { message = "Username or password cannot be empty." });
            }

            var usernameTrimmedLower = login.username?.Trim().ToLower() ?? string.Empty;
            int userCount = await _context.Users
                            .Where(u => (u.Uname ?? "").Trim().ToLower() == usernameTrimmedLower)
                            .CountAsync();

            if (userCount == 0 && !IsPredefinedUser(usernameTrimmedLower))
            {
                _logger.LogWarning("User not found: {Username}", login.username);
                return NotFound(new { message = "User not found." });
            }

            try
            {
                // Decrypt the password if it is encrypted
                if (login.isPasswordEncrypted)
                {
                    if (IsBase64String(login.password))
                    {
                        login.password = LoginUtils.DecryptPassword(login.password);
                    }
                    else
                    {
                        _logger.LogWarning("Invalid Base-64 string received for user: {Username}", login.username);
                        return BadRequest(new { message = "Invalid Base-64 string for password." });
                    }
                }

                var authResult = await AuthenticateUserAsync(login.username!, login.password);
                if (!authResult.IsSuccess)
                {
                    _logger.LogWarning("Authentication failed for user: {Username}", login.username);
                    return StatusCode(401, new { message = "Authentication failed." });
                }

                int userid = await _userService.GetUserIdAsync(usernameTrimmedLower);
                var token = _tokenService.GenerateJwtToken(login.username!);

                // Re-encrypt the password before sending it back to the client
                login.password = LoginUtils.EncryptPassword(login.password);
                login.isPasswordEncrypted = true; // Indicate that the password is now encrypted

                login.userid = userid.ToString();
                login.token = token;

                _logger.LogInformation("Login successful for user: {Username}", login.username);
                return Ok(login);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for user: {Username}", login.username);
                return StatusCode(500, new { message = "An internal error occurred. Please try again later." });
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
                _logger.LogError(ex, "Failed to authenticate user: {Username}", username);
                return AuthenticationResult.Failure();
            }
        }

        private static bool IsPredefinedUser(string username) => predefinedUserArray.Contains(username);

        private static bool IsBase64String(string base64)
        {
            Span<byte> buffer = new(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }
    }
}