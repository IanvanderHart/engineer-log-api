using EngineerLog.Api.Data;
using EngineerLog.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EngineerLog.Api.Services;

public class LogEntryService : ILogEntryService
{
    private readonly AppDbContext _context;

    public LogEntryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LogEntry> CreateLogEntryAsync(LogEntry entry)
    {
        // 1. Бизнес-правило: Проверяем, существует ли проект, к которому мы привязываем запись
        var projectExists = await _context.Projects.AnyAsync(p => p.Id == entry.ProjectId);
        if (!projectExists)
        {
            throw new ArgumentException($"Проект с ID {entry.ProjectId} не найден", nameof(entry.ProjectId));
        }

        // 2. Бизнес-правило: Если дата не указана, ставим текущее время UTC
        if (entry.CreatedAt == default)
        {
            entry.CreatedAt = DateTime.UtcNow;
        }

        // 3. Сохраняем
        _context.LogEntries.Add(entry);
        await _context.SaveChangesAsync();

        return entry;
    }

    public async Task<List<LogEntry>> GetLogEntriesByProjectIdAsync(int projectId)
    {
        // Возвращаем записи, отсортированные по дате создания (новые сверху)
        return await _context.LogEntries
            .Where(e => e.ProjectId == projectId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }
        public async Task<LogEntry?> UpdateLogEntryAsync(int id, LogEntry updatedEntry)
    {
        // 1. Ищем существующую запись
        var existingEntry = await _context.LogEntries.FindAsync(id);
        if (existingEntry == null)
        {
            return null; // Возвращаем null, а контроллер превратит это в 404 Not Found
        }

        // 2. Обновляем только разрешенные поля (защита от подмены ID или ProjectId)
        existingEntry.Title = updatedEntry.Title;
        existingEntry.Description = updatedEntry.Description;
        existingEntry.Solution = updatedEntry.Solution;
        existingEntry.Type = updatedEntry.Type;
        
        // 3. Фиксируем время обновления (хорошая практика!)
        existingEntry.UpdatedAt = DateTime.UtcNow;

        // 4. Сохраняем
        await _context.SaveChangesAsync();
        return existingEntry;
    }

    public async Task<bool> DeleteLogEntryAsync(int id)
    {
        var existingEntry = await _context.LogEntries.FindAsync(id);
        if (existingEntry == null)
        {
            return false;
        }

        _context.LogEntries.Remove(existingEntry);
        await _context.SaveChangesAsync();
        return true;
    }
}

