using AirwayAPI.Models;
using AirwayAPI.Models.MassMailerModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AirwayAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<int> GetUserIdAsync(string username);
        Task<string> GetUsernameAsync(int userId);

        // Returns full User model data for all active users.
        Task<IEnumerable<User>> GetActiveUsersAsync();

        // Returns only active users (from GetActiveUsersAsync) who have sent at least one MassMailer,
        // projected into the MassMailerUser model.
        Task<IEnumerable<MassMailerUser>> GetMassMailerUsersAsync();
    }
}
