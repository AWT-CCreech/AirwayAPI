﻿using AirwayAPI.Application;
using AirwayAPI.Data;
using AirwayAPI.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace AirwayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserLoginsController : ControllerBase
    {
        private readonly eHelpDeskContext _context;
        private readonly ILogger<UserLoginsController> _logger;

        public UserLoginsController(eHelpDeskContext context, ILogger<UserLoginsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<LoginInfo>> GetUsers([FromBody] LoginInfo login)
        {
            if (string.IsNullOrWhiteSpace(login.username) || string.IsNullOrWhiteSpace(login.password))
            {
                _logger.LogWarning("Invalid login request: username or password is missing.");
                return BadRequest(new { message = "Username or password cannot be empty." });
            }

            var usernameTrimmedLower = login.username.Trim().ToLower();
            int userCount = await _context.Users
                                          .Where(u => (u.Uname ?? string.Empty).Trim().ToLower() == usernameTrimmedLower)
                                          .CountAsync();

            if (userCount == 0 && !IsPredefinedUser(usernameTrimmedLower))
            {
                _logger.LogWarning("User not found: {Username}", login.username);
                return NotFound(new { message = "User not found." });
            }

            try
            {
                if (login.isPasswordEncrypted)
                {
                    if (IsBase64String(login.password))
                    {
                        login.password = LoginUtils.decryptPassword(login.password);
                    }
                    else
                    {
                        _logger.LogWarning("Invalid Base-64 string received for user: {Username}", login.username);
                        return BadRequest(new { message = "Invalid Base-64 string for password." });
                    }
                }

                var authResult = await AuthenticateUserAsync(login.username, login.password);
                if (!authResult.IsSuccess)
                {
                    _logger.LogWarning("Authentication failed for user: {Username}", login.username);
                    return StatusCode(401, new { message = "Authentication failed." });
                }

                int userid = await GetUserIdAsync(usernameTrimmedLower);

                var user = new LoginInfo()
                {
                    userid = userid.ToString(),
                    username = login.username,
                    password = LoginUtils.encryptPassword(login.password)
                };

                _logger.LogInformation("Login successful for user: {Username}", login.username);
                return Ok(user);
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

        private async Task<int> GetUserIdAsync(string username)
        {
            return username switch
            {
                "mgibson" => 125,
                "lvonder" => 65,
                "kgildersleeve" => 229,
                _ => await _context.Users
                                   .Where(u => (u.Uname ?? string.Empty).Trim().ToLower() == username)
                                   .Select(u => u.Id)
                                   .FirstOrDefaultAsync()
            };
        }

        private static bool IsPredefinedUser(string username) =>
            new[] { "mgibson", "lvonder", "kgildersleeve" }.Contains(username);

        private static bool IsBase64String(string base64)
        {
            Span<byte> buffer = new(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }

        private class AuthenticationResult
        {
            public bool IsSuccess { get; private set; }
            public static AuthenticationResult Success() => new() { IsSuccess = true };
            public static AuthenticationResult Failure() => new() { IsSuccess = false };
        }
    }
}
