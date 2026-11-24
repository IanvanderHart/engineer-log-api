namespace EngineerLog.Api.Models;

public class User
{
    public int Id { get; set; }
    
    // Уникальное имя пользователя (логин)
    public string Username { get; set; } = string.Empty;
    
    // Хеш пароля (никогда не храним сам пароль!)
    public string PasswordHash { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
