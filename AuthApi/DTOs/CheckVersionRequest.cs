namespace AuthApi.DTOs;

public class CheckVersionRequest
{
    public string Product { get; set; } = "";
    public string Version { get; set; } = "";
}