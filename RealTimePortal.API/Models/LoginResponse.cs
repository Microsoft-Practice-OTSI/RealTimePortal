namespace RealTimePortal.API.Models;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public int ExpiresIn { get; set; }
}