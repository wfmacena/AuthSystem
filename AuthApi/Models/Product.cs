namespace AuthApi.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public string RequiredVersion { get; set; } = "1.0.0";
    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}