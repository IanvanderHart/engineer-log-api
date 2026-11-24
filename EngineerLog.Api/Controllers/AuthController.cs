using Microsoft.AspNetCore.Mvc;
using EngineerLog.Api.Services;

namespace EngineerLog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request.Username, request.Password);
            return Ok(new { message = result });
        }
        catch (InvalidOperationException ex)
        {
            // Возвращаем 400 Bad Request, если пользователь уже существует
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var token = await _authService.LoginAsync(request.Username, request.Password);
            // Возвращаем токен клиенту
            return Ok(new { token });
        }
        catch (InvalidOperationException ex)
        {
            // Возвращаем 401 Unauthorized при неверных данных
            return Unauthorized(new { message = ex.Message });
        }
    }
}

// DTO (Data Transfer Objects) для входящих запросов
// 'record' в C# автоматически создает неизменяемый класс с конструктором и свойствами
public record RegisterRequest(string Username, string Password);
public record LoginRequest(string Username, string Password);
