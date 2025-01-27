namespace AirwayAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<int> GetUserIdAsync(string username);
    }
}

