namespace AirwayAPI.Models.LoginModels;

public class LoginInfo
{
    public string userid { get; set; }
    public string username { get; set; }
    public string password { get; set; }
    public bool isPasswordEncrypted { get; set; }
    public string token { get; set; }
    public string refreshToken { get; set; }   // <— added for refresh‑token support
}
