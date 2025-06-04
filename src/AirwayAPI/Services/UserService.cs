using AirwayAPI.Data;
using AirwayAPI.Models;
using AirwayAPI.Models.MassMailerModels;
using AirwayAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AirwayAPI.Services;

public class UserService(eHelpDeskContext context) : IUserService
{
    private readonly eHelpDeskContext _context = context;

    public async Task<int> GetUserIdAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return 0;

        username = username.Trim().ToLower();

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

    public async Task<string> GetUsernameAsync(int userId)
    {
        if (userId <= 0)
            return string.Empty;

        var username = userId switch
        {
            125 => "mgibson",
            65 => "lvonder",
            229 => "kgildersleeve",
            _ => await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.Uname)
                    .FirstOrDefaultAsync()
        };

        return username ?? string.Empty;
    }

    // Returns full User model data for all active users.
    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        var users = await _context.Users
            .OrderBy(u => u.Uname)
            .Where(u => u.Active == 1
                    && u.Fname != null
                    && u.Lname != null
                    && u.Lname != "ogitel"
                    && !u.Lname.Contains("assigned")
                    && !u.Lname.Contains("House")
                    && !u.Lname.Contains("-")
                    && !u.Fname.StartsWith("Am")
                    && !u.Fname.StartsWith("Ac")
                    && !u.Fname.StartsWith("Ae")
                    && !u.Fname.EndsWith("AC")
            )
            .ToListAsync();

        // Then filter in memory using Regex for digits (or special characters).
        var filteredUsers = users
            .Where(u => !Regex.IsMatch(u.Lname!, @"\d"))
            .ToList();

        return filteredUsers;
    }

    public async Task<IEnumerable<User>> GetScanUsersAsync()
    {
        var excluded = new[] { "testlab" };

        // 1) database does only Active=1, DeptID=8 and join with ScanHistory
        var dbList = await (
            from u in _context.Users
            join s in _context.ScanHistories
                on u.Uname!.Trim().ToLower() equals s.UserName!.Trim().ToLower()
            where u.Active == 1
               && u.DeptId == 8
            select u
        )
        .Distinct()
        .OrderBy(u => u.Uname)
        .ToListAsync();

        // 2) in‐memory exclude the unwanted uname(s)
        var result = dbList
            .Where(u => !excluded.Contains(u.Uname!.Trim().ToLower()))
            .ToList();

        return result;
    }


    // Leverages GetActiveUsersAsync to filter out only those users who have sent a MassMailer.
    // Projects the result into the MassMailerUser model.
    public async Task<IEnumerable<MassMailerUser>> GetMassMailerUsersAsync()
    {
        var activeUsers = await GetActiveUsersAsync();

        var massMailerUsers = activeUsers
            .Where(u => _context.MassMailers.Any(m => m.SentBy == u.Id))
            .Select(u => new MassMailerUser
            {
                UserName = u.Uname!.ToLower(),
                FullName = u.Fname + " " + u.Lname,
                Email = (u.Email ?? string.Empty).ToLower()
            });

        return massMailerUsers;
    }
}
