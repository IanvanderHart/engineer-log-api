using Microsoft.EntityFrameworkCore;
using EngineerLog.Api.Data;
using EngineerLog.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Добавляем контроллеры
builder.Services.AddControllers();

// 2. Добавляем Swagger для удобного тестирования API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Настраиваем подключение к PostgreSQL 
// (Пока используем локальную строку, позже заменим на реальную для сервера)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. РЕГИСТРИРУЕМ НАШ СЕРВИС (Scoped означает: один экземпляр на один HTTP-запрос)
builder.Services.AddScoped<IProjectService, ProjectService>();
// Добавляем наш новый сервис
builder.Services.AddScoped<ILogEntryService, LogEntryService>();

var app = builder.Build();

// Настраиваем пайплайн (middleware)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
