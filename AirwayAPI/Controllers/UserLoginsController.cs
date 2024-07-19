using AirwayAPI.Application;
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
            //_logger.LogInformation("Received login request for user: {Username}", login.username);
            //_logger.LogInformation("Payload: {Payload}", Newtonsoft.Json.JsonConvert.SerializeObject(login));

            if (string.IsNullOrWhiteSpace(login.username) || string.IsNullOrWhiteSpace(login.password))
            {
                _logger.LogWarning("Invalid login request: username or password is missing.");
                return BadRequest(new { message = "Username or password cannot be empty." });
            }

            int dbCheck = await _context.Users
                                .Where(u => (u.Uname ?? string.Empty).Trim().ToLower() == login.username.Trim().ToLower())
                                .CountAsync();

            if (dbCheck == 0 && !(new[] { "mgibson", "lvonder", "kgildersleeve" }.Contains(login.username.Trim().ToLower())))
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

                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync($"{login.username}@airway.com", login.password);

                int userid = login.username.Trim().ToLower() switch
                {
                    "mgibson" => 125,
                    "lvonder" => 65,
                    "kgildersleeve" => 229,
                    _ => await _context.Users
                                        .Where(u => (u.Uname ?? string.Empty).Trim().ToLower() == login.username.Trim().ToLower())
                                        .Select(u => u.Id)
                                        .FirstOrDefaultAsync()
                };

                var user = new LoginInfo()
                {
                    userid = userid.ToString(),
                    username = login.username,
                    password = LoginUtils.encryptPassword(login.password)
                };

                await client.DisconnectAsync(true);

                _logger.LogInformation("Login successful for user: {Username}", login.username);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication failed for user: {Username}", login.username);
                return StatusCode(500, new { message = "Authentication failed." });
            }
        }

        private static bool IsBase64String(string base64)
        {
            Span<byte> buffer = new(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }
    }
}
