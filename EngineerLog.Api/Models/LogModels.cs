namespace EngineerLog.Api.Models;

public class Project
{
    public int Id { get; set; }
    
    // Используем первичный конструктор C# 12 для лаконичности, 
    // но с явной инициализацией пустой строкой для безопасности от null
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Навигационное свойство: EF Core автоматически поймет, что это связь "Один ко многим"
    public ICollection<LogEntry> Entries { get; set; } = new List<LogEntry>();
}

public class LogEntry
{
    public int Id { get; set; }
    
    // Внешний ключ (связывает запись с конкретным проектом)
    public int ProjectId { get; set; }
    
    // Навигационное свойство обратно к проекту
    public Project? Project { get; set; } 

    public EntryType Type { get; set; } 
    
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Solution { get; set; }

    // Правильная работа с датой! 
    // DateTime.UtcNow гарантирует, что время будет в едином стандарте, без проблем с часовыми поясами.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

// Enum (перечисление) делает код читаемым и защищает от некорректных строковых значений ("Event", "Problem")
public enum EntryType
{
    Event,    // Событие / Фича
    Problem   // Проблема / Баг
}
