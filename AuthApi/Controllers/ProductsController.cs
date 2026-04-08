using AuthApi.Data;
using AuthApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products
            .OrderBy(p => p.Id)
            .ToListAsync();

        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] Product product)
    {
        var exists = await _context.Products.AnyAsync(p => p.Name == product.Name);
        if (exists)
            return BadRequest(new { success = false, error = "Produto já existe" });

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Produto criado com sucesso" });
    }
}