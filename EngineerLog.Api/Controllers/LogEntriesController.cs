using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EngineerLog.Api.Models;
using EngineerLog.Api.Services;

namespace EngineerLog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LogEntriesController : ControllerBase
{
    private readonly ILogEntryService _logEntryService;

    public LogEntriesController(ILogEntryService logEntryService)
    {
        _logEntryService = logEntryService;
    }

// GET: api/logentries/project/1?categoryId=2&tagIds=1,3,5&search=ошибка
    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<List<LogEntry>>> GetByProject(
        int projectId, 
        [FromQuery] int? categoryId = null,
        [FromQuery] string? tagIds = null,
        [FromQuery] string? search = null)
    {
        // Преобразуем строку tagIds "1,3,5" в List<int>
        List<int>? tagIdList = null;
        if (!string.IsNullOrWhiteSpace(tagIds))
        {
            tagIdList = tagIds.Split(',')
                .Select(id => int.TryParse(id.Trim(), out var parsedId) ? parsedId : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();
        }

        var entries = await _logEntryService.GetLogEntriesByProjectIdAsync(
            projectId, 
            categoryId, 
            tagIdList, 
            search);
            
        return Ok(entries);
    }

// POST: api/logentries
       [HttpPost]
       public async Task<IActionResult> Create([FromBody] EngineerLog.Api.DTOs.CreateLogEntryRequest request)
       {
           try
           {
               // Просто делегируем работу сервису
               var createdEntry = await _logEntryService.CreateLogEntryAsync(request);
               
               // Возвращаем 201 Created с данными записи
               return StatusCode(201, createdEntry);
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
