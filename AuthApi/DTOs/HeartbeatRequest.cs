namespace AuthApi.DTOs;

public class HeartbeatRequest
{
    public string Login { get; set; } = "";
    public string Hwid { get; set; } = "";
    public string Product { get; set; } = "";
}