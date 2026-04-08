namespace AuthApi.Models;

public class User
{
    public int Id { get; set; }

    public string Login { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Member";

    public string? Hwid { get; set; }
    public bool HwidLoginEnabled { get; set; } = false;

    public DateTime? ExpiresAt { get; set; }

    public bool IsOnline { get; set; } = false;
    public string? LastIp { get; set; }
    public DateTime? LastSeenAt { get; set; }

    public int LoginCount { get; set; } = 0;

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}