const API_URL = '/api';

// Функция для получения токена из localStorage
const getToken = () => localStorage.getItem('token');

// Универсальная функция для запросов


export async function apiRequest(endpoint: string, options: RequestInit = {}) {
  const token = getToken();
  
  const headers = {
    'Content-Type': 'application/json',
    ...(token ? { 'Authorization': `Bearer ${token}` } : {}),
    ...options.headers,
  };

  const response = await fetch(`${API_URL}${endpoint}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({}));
    throw new Error(errorData.message || `Ошибка HTTP: ${response.status}`);
  }

  // === НОВАЯ МАГИЯ: Если ответ пустой (например, 204 No Content при удалении), возвращаем null ===
  if (response.status === 204 || response.headers.get("content-length") === "0") {
    return null;
  }
  // ==========================================================================================

  return response.json();
}
