using AirwayAPI.Data;
using AirwayAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirwayAPI.Services
{
    public class UserService(eHelpDeskContext context) : IUserService
    {
        private readonly eHelpDeskContext _context = context;

        public async Task<int> GetUserIdAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return 0;

            // Normalize username for comparison
            username = username.Trim().ToLower();

            // Return hard-coded IDs for specific known usernames
            return username switch
            {
                "mgibson" => 125,
                "lvonder" => 65,
                "kgildersleeve" => 229,

                // Otherwise, lookup in DB
                _ => await _context.Users
                    .Where(u => (u.Uname ?? string.Empty).Trim().ToLower() == username)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync()
            };
        }
    }
}
