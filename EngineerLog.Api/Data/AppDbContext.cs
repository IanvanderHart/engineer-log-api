using Microsoft.EntityFrameworkCore;
using EngineerLog.Api.Models;

namespace EngineerLog.Api.Data;

public class AppDbContext : DbContext
{
    // Конструктор, который принимает настройки (строку подключения и т.д.)
    // Это пример внедрения зависимостей (Dependency Injection) на уровне фреймворка
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Свойства DbSet представляют собой таблицы в базе данных
    public DbSet<Project> Projects { get; set; }
    public DbSet<LogEntry> LogEntries { get; set; }

    // Переопределение метода (override) для тонкой настройки маппинга
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Настраиваем поведение при удалении: 
        // Если удаляется Project, все связанные с ним LogEntry тоже удаляются (CASCADE).
        // Это защищает базу данных от "висячих" записей (orphan records).
        modelBuilder.Entity<LogEntry>()
            .HasOne(e => e.Project)
            .WithMany(p => p.Entries)
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
