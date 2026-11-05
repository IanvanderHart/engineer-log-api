using EngineerLog.Api.Models;

namespace EngineerLog.Api.Services;

public interface ILogEntryService
{
    Task<LogEntry> CreateLogEntryAsync(LogEntry entry);
    Task<List<LogEntry>> GetLogEntriesByProjectIdAsync(int projectId);
    Task<LogEntry?> UpdateLogEntryAsync(int id, LogEntry updateEntry);
    Task<bool> DeleteLogEntryAsync(int id);
}
