using AuthApi.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] AdminLoginRequest request)
    {
        var adminUser = "plugadmin";
        var adminPass = "123456wm";

        if (request.Username != adminUser || request.Password != adminPass)
        {
            return Unauthorized(new
            {
                success = false,
                error = "Usuário ou senha inválidos"
            });
        }

        return Ok(new
        {
            success = true,
            token = "admin-logado"
        });
    }
}