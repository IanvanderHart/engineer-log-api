using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using EngineerLog.Api.Data;
using EngineerLog.Api.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// === НАСТРОЙКА JWT ===
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(secretKey)
        };
    });

// === НАСТРОЙКА CORS ===
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("AllowFrontend", policy =>
       {
           policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowCredentials();
       });
   });

builder.Services.AddAuthorization(); // Включаем авторизацию

// 1. Добавляем контроллеры
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Игнорируем циклические ссылки (Project -> Entries -> Project)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // 2. Разрешаем сериализацию и десериализацию Enum как строк ("Event", "Problem")
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        
        // (Опционально) Делаем JSON читаемым при отладке
        options.JsonSerializerOptions.WriteIndented = true; 
    });

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
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Настраиваем пайплайн (middleware)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
