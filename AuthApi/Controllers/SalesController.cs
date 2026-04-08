using AuthApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly AppDbContext _context;

    public SalesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSales()
    {
        var sales = await _context.Sales
            .Include(s => s.User)
            .Include(s => s.Product)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new
            {
                s.Id,
                User = s.User != null ? s.User.Login : "-",
                Product = s.Product != null ? s.Product.Name : "-",
                s.Amount,
                s.CreatedAt
            })
            .ToListAsync();

        return Ok(sales);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var totalSales = await _context.Sales.CountAsync();
        var totalRevenue = await _context.Sales.SumAsync(s => (decimal?)s.Amount) ?? 0;

        var salesByProduct = await _context.Sales
            .Include(s => s.Product)
            .GroupBy(s => s.Product != null ? s.Product.Name : "Sem produto")
            .Select(g => new
            {
                Product = g.Key,
                Count = g.Count(),
                Total = g.Sum(x => x.Amount)
            })
            .ToListAsync();

        return Ok(new
        {
            totalSales,
            totalRevenue,
            salesByProduct
        });
    }
}