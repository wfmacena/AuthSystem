using AuthApi.Data;
using AuthApi.DTOs;
using AuthApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("verificar-versao")]
    [HttpPost("check-version")]
    [HttpPost("verificar-versão")]
    public async Task<IActionResult> VerificarVersao([FromBody] CheckVersionRequest request)
    {
        var productName = request.Product?.Trim();
        var version = request.Version?.Trim();

        if (string.IsNullOrWhiteSpace(productName))
        {
            return BadRequest(new
            {
                success = false,
                error = "Produto vazio",
                receivedProduct = request.Product,
                receivedVersion = request.Version
            });
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(p =>
                p.Name.ToLower() == productName.ToLower() &&
                p.IsActive);

        if (product == null)
        {
            return BadRequest(new
            {
                success = false,
                error = "Produto inválido",
                receivedProduct = request.Product,
                normalizedProduct = productName
            });
        }

        if (product.RequiredVersion != version)
        {
            return Ok(new
            {
                success = false,
                requiredVersion = product.RequiredVersion,
                receivedVersion = version
            });
        }

        return Ok(new
        {
            success = true,
            receivedProduct = productName,
            receivedVersion = version
        });
    }

    [HttpPost("auth")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Product)
            .FirstOrDefaultAsync(u => u.Login == request.Login);

        if (user == null)
            return Unauthorized(new { success = false, error = "Usuário não encontrado" });

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { success = false, error = "Senha inválida" });

        if (user.Product == null || user.Product.Name != request.Product)
            return BadRequest(new { success = false, error = "Produto inválido para esse usuário" });

        if (user.ExpiresAt.HasValue && user.ExpiresAt.Value < DateTime.UtcNow)
            return BadRequest(new { success = false, error = "Acesso expirado" });

        if (!string.IsNullOrWhiteSpace(user.Hwid) && user.Hwid != request.Hwid)
            return BadRequest(new { success = false, error = "HWID inválido" });

        if (string.IsNullOrWhiteSpace(user.Hwid))
            user.Hwid = request.Hwid;

        user.IsOnline = true;
        user.LastSeenAt = DateTime.UtcNow;
        user.LastIp = HttpContext.Connection.RemoteIpAddress?.ToString();

        _context.LoginLogs.Add(new LoginLog
        {
            UserId = user.Id,
            Ip = user.LastIp,
            Hwid = request.Hwid,
            Success = true
        });

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            username = user.Login,
            expiry = user.ExpiresAt?.ToString("yyyy-MM-dd") ?? "Lifetime",
            role = user.Role,
            product = user.Product.Name
        });
    }

    [HttpPost("auth-by-hwid")]
    public async Task<IActionResult> AuthByHwid(AuthByHwidRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Product)
            .FirstOrDefaultAsync(u => u.Hwid == request.Hwid && u.HwidLoginEnabled);

        if (user == null)
            return Unauthorized(new { success = false, error = "HWID não autorizado" });

        if (user.ExpiresAt.HasValue && user.ExpiresAt.Value < DateTime.UtcNow)
            return BadRequest(new { success = false, error = "Acesso expirado" });

        user.IsOnline = true;
        user.LastSeenAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            username = user.Login,
            expiry = user.ExpiresAt?.ToString("yyyy-MM-dd") ?? "Lifetime",
            role = user.Role,
            product = user.Product!.Name
        });
    }

    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat(HeartbeatRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Product)
            .FirstOrDefaultAsync(u =>
                u.Login == request.Login &&
                u.Hwid == request.Hwid &&
                u.Product != null &&
                u.Product.Name == request.Product);

        if (user == null)
            return Unauthorized(new { success = false, error = "Sessão inválida" });

        user.IsOnline = true;
        user.LastSeenAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(HeartbeatRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Product)
            .FirstOrDefaultAsync(u =>
                u.Login == request.Login &&
                u.Hwid == request.Hwid &&
                u.Product != null &&
                u.Product.Name == request.Product);

        if (user == null)
            return Unauthorized(new { success = false, error = "Sessão inválida" });

        user.IsOnline = false;
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    [HttpPost("save-hwid-login")]
    public async Task<IActionResult> SaveHwidLogin(SaveHwidLoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Product)
            .FirstOrDefaultAsync(u =>
                u.Login == request.Login &&
                u.Hwid == request.Hwid &&
                u.Product != null &&
                u.Product.Name == request.Product);

        if (user == null)
            return Unauthorized(new { success = false, error = "Usuário inválido" });

        user.HwidLoginEnabled = request.Enabled;
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    [HttpPost("get-hwid-login")]
    public async Task<IActionResult> GetHwidLogin(HeartbeatRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Product)
            .FirstOrDefaultAsync(u =>
                u.Login == request.Login &&
                u.Hwid == request.Hwid &&
                u.Product != null &&
                u.Product.Name == request.Product);

        if (user == null)
            return Unauthorized(new { success = false, error = "Usuário inválido" });

        return Ok(new { success = true, enabled = user.HwidLoginEnabled });
    }
}