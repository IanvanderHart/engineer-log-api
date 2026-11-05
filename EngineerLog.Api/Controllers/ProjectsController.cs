using Microsoft.AspNetCore.Mvc;
using EngineerLog.Api.Models;
using EngineerLog.Api.Services;

namespace EngineerLog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    // Внедряем интерфейс сервиса, а не конкретный класс. 
    // Это позволяет легко подменять реализацию (например, для тестов).
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    // GET: api/projects
    [HttpGet]
    public async Task<ActionResult<List<Project>>> GetAll()
    {
        var projects = await _projectService.GetAllProjectsAsync();
        return Ok(projects);
    }

    // GET: api/projects/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Project>> GetById(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        
        if (project == null)
        {
            return NotFound(new { message = $"Проект с ID {id} не найден" });
        }

        return Ok(project);
    }

    // POST: api/projects
    [HttpPost]
    public async Task<ActionResult<Project>> Create([FromBody] Project newProject)
    {
        try
        {
            var createdProject = await _projectService.CreateProjectAsync(newProject);
            // Возвращаем код 201 Created и ссылку на созданный ресурс
            return CreatedAtAction(nameof(GetById), new { id = createdProject.Id }, createdProject);
        }
        catch (ArgumentException ex)
        {
            // Ловим нашу бизнес-ошибку из сервиса (например, пустое имя)
            return BadRequest(new { message = ex.Message });
        }
    }
}
