# Этап 1: Сборка
   FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
   WORKDIR /src
   COPY . .
   RUN dotnet restore
   RUN dotnet publish -c Release -o /app/publish

   # Этап 2: Запуск
   FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
   WORKDIR /app
   COPY --from=build /app/publish .
   
   # Указываем, что приложение должно слушать порт 5036
   ENV ASPNETCORE_URLS=http://+:5036
   EXPOSE 5036
   
   ENTRYPOINT ["dotnet", "EngineerLog.Api.dll"]
