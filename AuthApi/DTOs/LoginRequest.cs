namespace AuthApi.DTOs;

public class LoginRequest
{
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
    public string Hwid { get; set; } = "";
    public string Product { get; set; } = "";
}