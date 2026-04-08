namespace AuthApi.DTOs;

public class UpdateUserRequest
{
    public string Login { get; set; } = "";
    public string? Password { get; set; }
    public int ProductId { get; set; }
    public string Role { get; set; } = "Member";
    public string TimeOption { get; set; } = "1 semana";
    public DateTime? CustomDate { get; set; }
}