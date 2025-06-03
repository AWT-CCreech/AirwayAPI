using AirwayAPI.Models;
using AirwayAPI.Models.MassMailerModels;

namespace AirwayAPI.Services.Interfaces;

public interface IUserService
{
    Task<int> GetUserIdAsync(string username);
    Task<string> GetUsernameAsync(int userId);

    // Returns full User model data for all active users.
    Task<IEnumerable<User>> GetActiveUsersAsync();

    // Returns only active users (from GetActiveUsersAsync) who have sent at least one MassMailer,
    // projected into the MassMailerUser model.
    Task<IEnumerable<MassMailerUser>> GetMassMailerUsersAsync();

    /// <summary>
    /// Returns all active warehouse users (DeptID = 8) who have at least one scan.
    /// </summary>
    Task<IEnumerable<User>> GetScanUsersAsync();
}
