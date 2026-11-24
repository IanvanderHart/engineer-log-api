using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit; // Библиотека для тестов
using Microsoft.EntityFrameworkCore;
using EngineerLog.Api.Data;
using EngineerLog.Api.Models;
using EngineerLog.Api.Services;
using EngineerLog.Api.DTOs;

namespace EngineerLog.Api.Tests;

public class LogEntryServiceTests
{
    // Этот метод помогает создать "чистую" базу данных в памяти для каждого теста
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + System.Guid.NewGuid().ToString()) // Уникальная БД для каждого теста
            .Options;

        return new AppDbContext(options);
    }

    [Fact] // Атрибут [Fact] говорит фреймворку: "Это тест, запусти его!"
    public async Task GetLogEntriesByProjectIdAsync_WithSearchQuery_ReturnsOnlyMatchingEntries()
    {
        // === 1. ARRANGE (Подготовка / Изготовление детали и калибра) ===
        var context = GetInMemoryDbContext();
        
        // Добавляем тестовый проект
        context.Projects.Add(new Project { Id = 1, Name = "Тестовый проект", Description = "Тест" });
        
        // Добавляем ДВЕ записи: одна содержит слово "сервер", другая - нет
        context.LogEntries.Add(new LogEntry 
        { 
            Id = 1, ProjectId = 1, Title = "Падал сервер", Description = "Сервер упал из-за порта", Type = EntryType.Problem 
        });
        context.LogEntries.Add(new LogEntry 
        { 
            Id = 2, ProjectId = 1, Title = "Настройка ЧПУ", Description = "Наладил шпиндель", Type = EntryType.Event 
        });
        await context.SaveChangesAsync();

        var service = new LogEntryService(context);

        // === 2. ACT (Действие / Пропускаем деталь через калибр) ===
        // Мы ищем записи с проектом ID=1 и поисковым запросом "сервер" (в любом регистре)
        var result = await service.GetLogEntriesByProjectIdAsync(projectId: 1, searchQuery: "сервер");

        // === 3. ASSERT (Проверка / Смотрим на результат измерения) ===
        // Проверяем, что вернулась ровно 1 запись (а не 2)
        Assert.Single(result); 
        
        // Проверяем, что эта запись действительно та, что нам нужна
        Assert.Equal("Падал сервер", result[0].Title);
    }
    [Fact]
        public async Task GetLogEntriesByProjectIdAsync_WithMultipleTags_ReturnsOnlyEntriesWithAllTags()
        {
            // === 1. ARRANGE (Подготовка) ===
            var context = GetInMemoryDbContext();
            
            // Создаем и добавляем тэги в контекст
            var tag1 = new Tag { Id = 1, Name = "c#", Color = "#68217A" };
            var tag2 = new Tag { Id = 2, Name = "docker", Color = "#2496ED" };
            var tag3 = new Tag { Id = 3, Name = "linux", Color = "#F59E0B" };
            
            context.Tags.AddRange(tag1, tag2, tag3);
            
            // Запись 1: имеет тэги c# и docker
            var entry1 = new LogEntry { Id = 1, ProjectId = 1, Title = "Настройка CI/CD", Type = EntryType.Event, Tags = new List<Tag> { tag1, tag2 } };
            // Запись 2: имеет только тэг c#
            var entry2 = new LogEntry { Id = 2, ProjectId = 1, Title = "Простой код на C#", Type = EntryType.Event, Tags = new List<Tag> { tag1 } };
            
            context.LogEntries.AddRange(entry1, entry2);
            await context.SaveChangesAsync();
    
            var service = new LogEntryService(context);
    
            // === 2. ACT (Действие) ===
            // Ищем записи, которые содержат И tэг 1 (c#), И тэг 2 (docker)
            var result = await service.GetLogEntriesByProjectIdAsync(projectId: 1, tagIds: new List<int> { 1, 2 });
    
            // === 3. ASSERT (Проверка) ===
            // Должна вернуться только 1 запись (entry1), так как entry2 не имеет тэга docker
            Assert.Single(result);
            Assert.Equal("Настройка CI/CD", result[0].Title);
        }
    
        [Fact]
        public async Task CreateLogEntryAsync_WithInvalidProjectId_ThrowsArgumentException()
        {
            // === 1. ARRANGE (Подготовка) ===
            var context = GetInMemoryDbContext();
            var service = new LogEntryService(context);
            
            var request = new CreateLogEntryRequest
            {
                ProjectId = 999, // Такого проекта не существует в пустой БД
                Type = "Event",
                Title = "Тестовая запись",
                Description = "Описание"
            };
    
            // === 2. ACT & 3. ASSERT (Действие и Проверка) ===
            // Мы ожидаем, что метод выбросит исключение ArgumentException
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateLogEntryAsync(request));
            
            // Проверяем, что сообщение об ошибке содержит правильный текст
            Assert.Contains("Проект с ID 999 не найден", exception.Message);
        }
}
