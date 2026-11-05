#!/bin/bash
echo "🚀 Начинаем обновление EngineerLog API..."

# 1. Обновляем код из GitHub (по SSH, без пароля, как мы настроили!)
echo "📥 Загружаем последние изменения из Git..."
git pull origin main

# 2. Собираем новый Docker-образ прямо здесь (это быстро, так как слои кешируются)
echo "🏗 Собираем Docker-образ..."
docker build -t engineerlog-api ./EngineerLog.Api

# 3. Останавливаем и удаляем старый контейнер (игнорируем ошибки, если его нет)
echo "🛑 Останавливаем старый контейнер..."
docker stop my-engineerlog 2>/dev/null
docker rm my-engineerlog 2>/dev/null

# 4. Запускаем новый контейнер
echo "▶️ Запускаем новый контейнер..."
docker run -d --name my-engineerlog --network host \
-e ASPNETCORE_HTTP_PORTS=8081 \
-e ConnectionStrings__DefaultConnection="Host=127.0.0.1;Port=5434;Database=EngineerLogDb;Username=engineerlog;Password=12345" \
engineerlog-api

# 5. Проверяем, что всё работает
echo "✅ Проверка работоспособности..."
sleep 3 # Даем приложению 3 секунды на старт
curl -s http://localhost:8081/api/projects > /dev/null
if [ $? -eq 0 ]; then
    echo "🎉 Успех! API работает и отвечает на запросы."
else
    echo "⚠️ Внимание: API запустился, но не отвечает на запросы. Проверь логи: docker logs my-engineerlog"
fi
