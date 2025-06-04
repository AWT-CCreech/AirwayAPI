namespace AirwayAPI.Models.LoginModels;

public class LoginResponse
{
    public string Userid { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Token { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}