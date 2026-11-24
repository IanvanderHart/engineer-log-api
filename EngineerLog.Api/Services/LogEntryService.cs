using EngineerLog.Api.Data;
using EngineerLog.Api.Models;
using EngineerLog.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EngineerLog.Api.Services;

public class LogEntryService : ILogEntryService
{
    private readonly AppDbContext _context;

    public LogEntryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LogEntry> CreateLogEntryAsync(CreateLogEntryRequest request)
    {
        // 1. Проверяем, существует ли проект
        var projectExists = await _context.Projects.AnyAsync(p => p.Id == request.ProjectId);
        if (!projectExists)
        {
            throw new ArgumentException($"Проект с ID {request.ProjectId} не найден");
        }

        // 2. Маппим DTO в модель
        Enum.TryParse<EntryType>(request.Type, true, out var entryType);

        var newEntry = new LogEntry
        {
            ProjectId = request.ProjectId,
            CategoryId = request.CategoryId,
            Type = entryType,
            Title = request.Title,
            Description = request.Description,
            Solution = request.Solution,
            CreatedAt = DateTime.UtcNow
        };

        // 3. Привязываем тэги (если они переданы)
        if (request.TagIds != null && request.TagIds.Any())
        {
            var tags = await _context.Tags.Where(t => request.TagIds.Contains(t.Id)).ToListAsync();
            newEntry.Tags = tags;
        }

        // 4. Сохраняем в базу
        _context.LogEntries.Add(newEntry);
        await _context.SaveChangesAsync();

        // 5. Возвращаем запись с подгруженными связями (категория и тэги)
        return await _context.LogEntries
            .Include(e => e.Category)
            .Include(e => e.Tags)
            .FirstOrDefaultAsync(e => e.Id == newEntry.Id) ?? newEntry;
    }

    public async Task<List<LogEntry>> GetLogEntriesByProjectIdAsync(
            int projectId, 
            int? categoryId = null, 
            List<int>? tagIds = null, 
            string? searchQuery = null)
        {
            // Начинаем с базового запроса
            var query = _context.LogEntries
                .Where(e => e.ProjectId == projectId)
                .Include(e => e.Category)
                .Include(e => e.Tags)
                .AsQueryable();
    
            // Фильтр по категории
            if (categoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == categoryId.Value);
            }
    
            // Фильтр по тэгам (запись должна содержать ВСЕ выбранные тэги)
            // Фильтр по тэгам (запись должна содержать ВСЕ выбранные тэги)
            if (tagIds != null && tagIds.Any())
            {
                var requiredTagsCount = tagIds.Count;
                query = query.Where(e => e.Tags.Count(t => tagIds.Contains(t.Id)) == requiredTagsCount);
            }
    
            // Поиск по тексту (заголовок, описание, решение)
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var searchTerm = searchQuery.ToLower();
                query = query.Where(e => 
                    e.Title.ToLower().Contains(searchTerm) ||
                    e.Description.ToLower().Contains(searchTerm) ||
                    (e.Solution != null && e.Solution.ToLower().Contains(searchTerm))
                );
            }
    
            // Сортировка: новые записи сверху
            return await query.OrderByDescending(e => e.CreatedAt).ToListAsync();
        }

    public async Task<LogEntry?> UpdateLogEntryAsync(int id, LogEntry updatedEntry)
    {
        var existingEntry = await _context.LogEntries
            .Include(e => e.Category)
            .Include(e => e.Tags)
            .FirstOrDefaultAsync(e => e.Id == id);
            
        if (existingEntry == null)
        {
            return null;
        }

        existingEntry.Title = updatedEntry.Title;
        existingEntry.Description = updatedEntry.Description;
        existingEntry.Solution = updatedEntry.Solution;
        existingEntry.Type = updatedEntry.Type;
        existingEntry.CategoryId = updatedEntry.CategoryId;
        existingEntry.UpdatedAt = DateTime.UtcNow;

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
