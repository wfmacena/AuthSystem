using AuthApi.Data;
using AuthApi.DTOs;
using AuthApi.Helpers;
using AuthApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .Include(u => u.Product)
            .Select(u => new
            {
                u.Id,
                u.Login,
                Product = u.Product != null ? u.Product.Name : "",
                u.Role,
                ExpiresAt = u.ExpiresAt,
                u.LoginCount,
                u.LastIp,
                u.IsOnline,
                Hwid = string.IsNullOrWhiteSpace(u.Hwid) ? "Não registrado" : u.Hwid
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var exists = await _context.Users.AnyAsync(u => u.Login == request.Login);
        if (exists)
            return BadRequest(new { success = false, error = "Login já existe" });

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId);
        if (product == null)
            return BadRequest(new { success = false, error = "Produto não encontrado" });

        var plainPassword = string.IsNullOrWhiteSpace(request.Password)
            ? "123456"
            : request.Password;

        var user = new User
        {
            Login = request.Login,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword),
            Role = request.Role,
            ProductId = request.ProductId,
            ExpiresAt = ExpirationHelper.CalculateExpiration(request.TimeOption, request.CustomDate),
            IsOnline = false,
            LoginCount = 0
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        if (request.RegisterSale)
        {
            var sale = new Sale
            {
                UserId = user.Id,
                ProductId = product.Id,
                Amount = product.Price
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
        }

        return Ok(new
        {
            success = true,
            message = "Usuário criado com sucesso",
            userId = user.Id
        });
    }

    [HttpPost("{id}/reset-hwid")]
    public async Task<IActionResult> ResetHwid(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { success = false, error = "Usuário não encontrado" });

        user.Hwid = null;
        user.HwidLoginEnabled = false;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "HWID resetado com sucesso" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { success = false, error = "Usuário não encontrado" });

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Usuário removido com sucesso" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { success = false, error = "Usuário não encontrado" });

        var loginExists = await _context.Users.AnyAsync(u => u.Login == request.Login && u.Id != id);
        if (loginExists)
            return BadRequest(new { success = false, error = "Já existe outro usuário com esse login" });

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId);
        if (product == null)
            return BadRequest(new { success = false, error = "Produto não encontrado" });

        user.Login = request.Login;
        user.ProductId = request.ProductId;
        user.Role = request.Role;
        user.ExpiresAt = ExpirationHelper.CalculateExpiration(request.TimeOption, request.CustomDate);

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Usuário atualizado com sucesso" });
    }
}