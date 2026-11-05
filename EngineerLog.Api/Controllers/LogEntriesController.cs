using Microsoft.AspNetCore.Mvc;
using EngineerLog.Api.Models;
using EngineerLog.Api.Services;

namespace EngineerLog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogEntriesController : ControllerBase
{
    private readonly ILogEntryService _logEntryService;

    public LogEntriesController(ILogEntryService logEntryService)
    {
        _logEntryService = logEntryService;
    }

    // GET: api/logentries/project/1
    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<List<LogEntry>>> GetByProject(int projectId)
    {
        var entries = await _logEntryService.GetLogEntriesByProjectIdAsync(projectId);
        return Ok(entries);
    }

    // POST: api/logentries
    [HttpPost]
    public async Task<ActionResult<LogEntry>> Create([FromBody] LogEntry newEntry)
    {
        try
        {
            var createdEntry = await _logEntryService.CreateLogEntryAsync(newEntry);
            return CreatedAtAction(nameof(GetByProject), new { projectId = createdEntry.ProjectId }, createdEntry);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
        // PUT: api/logentries/1
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] LogEntry updatedEntry)
    {
        // Защищаем от попытки изменить ID в теле запроса
        if (id != updatedEntry.Id)
        {
            return BadRequest(new { message = "ID в URL и в теле запроса не совпадают" });
        }

        var result = await _logEntryService.UpdateLogEntryAsync(id, updatedEntry);
        
        if (result == null)
        {
            return NotFound(new { message = $"Запись с ID {id} не найдена" });
        }

        return Ok(result);
    }

    // DELETE: api/logentries/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var isDeleted = await _logEntryService.DeleteLogEntryAsync(id);
        
        if (!isDeleted)
        {
            return NotFound(new { message = $"Запись с ID {id} не найдена" });
        }

        return NoContent(); // Стандартный ответ 204 для успешного удаления
    }
}
