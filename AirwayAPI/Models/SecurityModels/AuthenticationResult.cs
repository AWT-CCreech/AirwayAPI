namespace AirwayAPI.Models.SecurityModels;

public class AuthenticationResult
{
    public bool IsSuccess { get; private set; }
    public static AuthenticationResult Success() => new() { IsSuccess = true };
    public static AuthenticationResult Failure() => new() { IsSuccess = false };
}
