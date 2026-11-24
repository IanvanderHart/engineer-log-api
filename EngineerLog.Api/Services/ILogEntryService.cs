using EngineerLog.Api.Models;
using EngineerLog.Api.DTOs;

namespace EngineerLog.Api.Services;

public interface ILogEntryService
{
    Task<LogEntry> CreateLogEntryAsync(CreateLogEntryRequest request);
    
    // ОБНОВЛЕННЫЙ МЕТОД: добавляем параметры фильтрации
    Task<List<LogEntry>> GetLogEntriesByProjectIdAsync(
        int projectId, 
        int? categoryId = null, 
        List<int>? tagIds = null, 
        string? searchQuery = null);
    
    Task<LogEntry?> UpdateLogEntryAsync(int id, LogEntry updatedEntry);
    Task<bool> DeleteLogEntryAsync(int id);
}
