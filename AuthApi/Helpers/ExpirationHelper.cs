namespace AuthApi.Helpers;

public static class ExpirationHelper
{
    public static DateTime? CalculateExpiration(string timeOption, DateTime? customDate = null)
    {
        var now = DateTime.UtcNow;

        return timeOption.ToLower() switch
        {
            "1 semana" => now.AddDays(7),
            "30 dias" => now.AddDays(30),
            "90 dias" => now.AddDays(90),
            "90 dias (trimestral)" => now.AddDays(90),
            "1 ano" => now.AddYears(1),
            "lifetime" => null,
            "custom" => customDate,
            "custom (escolher data)" => customDate,
            _ => now.AddDays(7)
        };
    }
}