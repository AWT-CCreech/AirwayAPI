using AirwayAPI.Application;
using AirwayAPI.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // POST: api/UserLogins
        [HttpPost]
        public async Task<ActionResult<LoginInfo>> GetUsers([FromBody] LoginInfo login)
        {
            _logger.LogInformation("Received login request for user: {Username}", login.username);

            if (string.IsNullOrWhiteSpace(login.username) || string.IsNullOrWhiteSpace(login.password))
            {
                _logger.LogWarning("Invalid login request: username or password is missing.");
                return BadRequest(new { message = "Username or password cannot be empty." });
            }

            int dbCheck = await _context.Users
                                .Where(u => (u.Uname ?? string.Empty).Trim().ToLower() == login.username.Trim().ToLower())
                                .CountAsync();

            LoginInfo user = new() { userid = "", username = "", password = "" };
            if (dbCheck > 0 || login.username.Trim().ToLower() == "mgibson"
                || login.username.Trim().ToLower() == "lvonder"
                || login.username.Trim().ToLower() == "kgildersleeve")
            {
                try
                {
                    SmtpClient client = new();

                    if (login.isPasswordEncrypted)
                    {
                        // Validate the Base-64 string
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

                    await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(login.username + "@airway.com", login.password);

                    int userid = 0;
                    if (login.username.Trim().ToLower() == "mgibson")
                    {
                        userid = 125;
                    }
                    else if (login.username.Trim().ToLower() == "lvonder")
                    {
                        userid = 65;
                    }
                    else if (login.username.Trim().ToLower() == "kgildersleeve")
                    {
                        userid = 229;
                    }
                    else
                    {
                        userid = await _context.Users
                                    .Where(u => (u.Uname ?? string.Empty).Trim().ToLower() == login.username.Trim().ToLower())
                                    .Select(u => u.Id).FirstAsync();
                    }

                    user = new LoginInfo()
                    {
                        userid = userid.ToString(),
                        username = login.username,
                        password = LoginUtils.encryptPassword(login.password)
                    };
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
                catch (Exception ex) // fail to authenticate
                {
                    _logger.LogError(ex, "Authentication failed for user: {Username}", login.username);
                    return StatusCode(500, new { message = "Authentication failed." });
                }
            }
            else
            {
                _logger.LogWarning("User not found: {Username}", login.username);
                return NotFound(new { message = "User not found." });
            }

            _logger.LogInformation("Login successful for user: {Username}", login.username);
            return Ok(user);
        }

        private static bool IsBase64String(string base64)
        {
            Span<byte> buffer = new(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }
    }
}
