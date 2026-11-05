using EngineerLog.Api.Data;
using EngineerLog.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EngineerLog.Api.Services;

// Класс реализует интерфейс. Он знает ВСЮ бизнес-логику.
public class ProjectService : IProjectService
{
    private readonly AppDbContext _context;

    // Внедрение зависимости через конструктор (Constructor Injection)
    // Мы не создаем new AppDbContext() внутри, мы просим фреймворк дать нам готовый экземпляр.
    public ProjectService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Project> CreateProjectAsync(Project project)
    {
        // 1. Бизнес-правило: проверяем валидность данных ДО обращения к БД
        if (string.IsNullOrWhiteSpace(project.Name))
        {
            throw new ArgumentException("Название проекта не может быть пустым", nameof(project.Name));
        }

        // 2. Добавляем в контекст
        _context.Projects.Add(project);
        
        // 3. Сохраняем изменения в БД (асинхронно, чтобы не блокировать поток)
        await _context.SaveChangesAsync();
        
        return project;
    }

    public async Task<List<Project>> GetAllProjectsAsync()
    {
        // Include(e => e.Entries) говорит EF Core: "Загрузи проект и сразу подтяни его записи"
        // Но для списка проектов пока можно без Include, чтобы не грузить лишнее
        return await _context.Projects.ToListAsync();
    }

    public async Task<Project?> GetProjectByIdAsync(int id)
    {
        return await _context.Projects
            .Include(p => p.Entries) // А вот здесь подтянем записи, чтобы отдать их клиенту
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
