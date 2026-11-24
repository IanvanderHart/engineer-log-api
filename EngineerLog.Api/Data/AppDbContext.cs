using Microsoft.EntityFrameworkCore;
using EngineerLog.Api.Models; // Убедись, что этот using есть в самом верху!

namespace EngineerLog.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Таблицы базы данных
    public DbSet<Project> Projects { get; set; }
    public DbSet<LogEntry> LogEntries { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Связь Многие-ко-многим: LogEntry <-> Tag
        // EF Core сам создаст промежуточную таблицу LogEntryTags
        modelBuilder.Entity<LogEntry>()
            .HasMany(e => e.Tags)
            .WithMany(t => t.Entries)
            .UsingEntity(j => j.ToTable("LogEntryTags"));

        // 2. Связь Один-ко-многим: Project -> LogEntry
        // Если удаляем Проект, удаляем и все его записи (Cascade)
        modelBuilder.Entity<LogEntry>()
            .HasOne(e => e.Project)
            .WithMany(p => p.Entries)
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // 3. Связь Один-ко-многим: Category -> LogEntry
        // ВАЖНО: Если удаляем Категорию, мы НЕ удаляем записи. 
        // Мы просто обнуляем CategoryId (SetNull), чтобы старые записи не потерялись.
        modelBuilder.Entity<LogEntry>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Entries)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.SetNull); 
    }
}
