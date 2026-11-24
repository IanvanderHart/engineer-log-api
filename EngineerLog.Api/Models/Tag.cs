namespace EngineerLog.Api.Models;

public class Tag
{
    public int Id { get; set; }
    
    // Название тэга: "fanuk", "siemens", "c#", "go", "linux"
    public string Name { get; set; } = string.Empty;
    
    // Цвет для красивого бейджа
    public string Color { get; set; } = "#6B7280";
    
    // Навигационное свойство: все записи с этим тэгом
    public ICollection<LogEntry> Entries { get; set; } = new List<LogEntry>();
}
