using System;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirwayAPI.Application;
using AirwayAPI.Data;
using AirwayAPI.Models;

namespace AirwayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserLoginsController : ControllerBase
    {
        private readonly eHelpDeskContext _context;

        public UserLoginsController(eHelpDeskContext context)
        {
            _context = context;
        }

        // GET: api/UserLogins
        [HttpPost]
        public async Task<LoginInfo> GetUsers(LoginInfo login)
        {
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
                        login.password = LoginUtils.decryptPassword(login.password);

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
                    Console.WriteLine(ex.Message);
                }
            }
            
            // if the user does not exist, return empty string
            return user;
        }
    }
}