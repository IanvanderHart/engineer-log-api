namespace EngineerLog.Api.Models;

public class Category
{
    public int Id { get; set; }
    
    // Название класса: "Производство", "Разработка", "CI/CD", "Linux"
    public string Name { get; set; } = string.Empty;
    
    // Краткое описание (необязательно)
    public string? Description { get; set; }
    
    // Цвет для красивого отображения бейджа (например, "#3B82F6")
    public string Color { get; set; } = "#6B7280";
    
    // Навигационное свойство: все записи этого класса
    public ICollection<LogEntry> Entries { get; set; } = new List<LogEntry>();
}
