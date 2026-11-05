# ЭТАП 1: Сборка (Build)
# Используем полный SDK .NET 10 для компиляции кода
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем файл проекта и восстанавливаем зависимости (это кешируется Docker'ом)
COPY ["EngineerLog.Api.csproj", "./"]
RUN dotnet restore "EngineerLog.Api.csproj"

# Копируем весь исходный код и компилируем его в режиме Release
COPY . .
RUN dotnet build "EngineerLog.Api.csproj" -c Release -o /app/build

# ЭТАП 2: Публикация (Publish)
# Собираем всё необходимое для запуска в одну папку
FROM build AS publish
RUN dotnet publish "EngineerLog.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ЭТАП 3: Финальный образ (Runtime)
# Используем легкий образ только для запуска (ASP.NET Runtime), без компилятора
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Копируем скомпилированные файлы из этапа публикации
COPY --from=publish /app/publish .

# Указываем порт, который будет слушать приложение внутри контейнера
EXPOSE 8080

# Задаем переменную окружения по умолчанию (можно будет переопределить при запуске)
ENV ASPNETCORE_HTTP_PORTS=8080

# Команда запуска приложения
ENTRYPOINT ["dotnet", "EngineerLog.Api.dll"]
