namespace AirwayAPI.Models.SecurityModels;

public class TokenInfo
{
    public string Token { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}