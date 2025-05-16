namespace AirwayAPI.Models.LoginModels;

public class LoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public bool IsPasswordEncrypted { get; set; }
}