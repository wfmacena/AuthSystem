namespace AuthApi.DTOs;

public class SaveHwidLoginRequest
{
    public string Login { get; set; } = "";
    public string Hwid { get; set; } = "";
    public string Product { get; set; } = "";
    public bool Enabled { get; set; }
}