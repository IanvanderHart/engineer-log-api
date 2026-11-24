using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EngineerLog.Api.Data;
using EngineerLog.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EngineerLog.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<string> RegisterAsync(string username, string password)
    {
        // 1. Проверяем, не занят ли логин
        if (await _context.Users.AnyAsync(u => u.Username == username))
        {
            throw new InvalidOperationException("Пользователь с таким именем уже существует");
        }

        // 2. ХЕШИРУЕМ пароль с помощью BCrypt (это безопасно и необратимо)
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        // 3. Создаем и сохраняем пользователя
        var user = new User
        {
            Username = username,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return "Пользователь успешно зарегистрирован";
    }

    public async Task<string> LoginAsync(string username, string password)
    {
        // 1. Ищем пользователя по имени
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
        {
            throw new InvalidOperationException("Неверное имя пользователя или пароль");
        }

        // 2. ПРОВЕРЯЕМ хеш: BCrypt сравнивает введенный пароль с хешем в базе
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            throw new InvalidOperationException("Неверное имя пользователя или пароль");
        }

        // 3. Если всё верно, генерируем JWT-токен
        return GenerateJwtToken(user);
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryInMinutes"])),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
