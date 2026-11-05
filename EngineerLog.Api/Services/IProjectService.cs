using EngineerLog.Api.Models;

namespace EngineerLog.Api.Services;

// Интерфейс — это наш контракт. Он говорит: "Любой класс, который меня реализует, 
// ОБЯЗАН уметь создавать и получать проекты".
public interface IProjectService
{
    Task<Project> CreateProjectAsync(Project project);
    Task<List<Project>> GetAllProjectsAsync();
    Task<Project?> GetProjectByIdAsync(int id);
}
