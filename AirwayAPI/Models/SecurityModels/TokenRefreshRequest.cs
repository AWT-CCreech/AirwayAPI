namespace AirwayAPI.Models.SecurityModels;

public class TokenRefreshRequest
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
}