import { useState, useEffect } from 'react';
import { apiRequest } from '../lib/api';

interface LogEntry {
  id: number;
  title: string;
  description: string;
  solution?: string; // <-- ДОБАВИЛИ это поле (необязательное)
  type: string; // 'Event' или 'Problem'
  createdAt: string;
  category?: { id: number; name: string; color: string };
  tags?: { id: number; name: string; color: string }[];
}

interface Project {
  id: number;
  name: string;
  description: string;
}

interface ProjectDetailProps {
  projectId: number;
  onBack: () => void;
}

export default function ProjectDetail({ projectId, onBack }: ProjectDetailProps) {
  const [project, setProject] = useState<Project | null>(null);
  const [entries, setEntries] = useState<LogEntry[]>([]);
  const [showAddForm, setShowAddForm] = useState(false);
  const [categories, setCategories] = useState<any[]>([]);
  const [tags, setTags] = useState<any[]>([]);
  const [selectedCategoryId, setSelectedCategoryId] = useState<number | ''>('');
  const [selectedFilterTagIds, setSelectedFilterTagIds] = useState<number[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadData();
  }, [projectId]);

const loadData = async () => {
     setIsLoading(true);
     setError('');
     try {
       const projectData = await apiRequest(`/projects/${projectId}`);
       setProject(projectData);
       
       const entriesData = await apiRequest(`/logentries/project/${projectId}`);
       setEntries(entriesData);
       
       const catsData = await apiRequest('/categories');
       setCategories(catsData);
       
       const tagsData = await apiRequest('/tags');
       setTags(tagsData);
     } catch (err: any) {
       console.error('Ошибка загрузки данных:', err);
       setError('Не удалось загрузить данные проекта');
     } finally {
       setIsLoading(false);
     }
   };

const loadFilteredData = async () => {
     setIsLoading(true);
     setError('');
     try {
       const params = new URLSearchParams();
       if (selectedCategoryId !== '') {
         params.append('categoryId', String(selectedCategoryId));
       }
       if (selectedFilterTagIds.length > 0) {
         params.append('tagIds', selectedFilterTagIds.join(','));
       }
       if (searchQuery.trim() !== '') {
         params.append('search', searchQuery.trim());
       }

       const queryString = params.toString();
       const url = `/logentries/project/${projectId}${queryString ? `?${queryString}` : ''}`;
       const entriesData = await apiRequest(url);
       setEntries(entriesData);
     } catch (err: any) {
       console.error('Ошибка загрузки отфильтрованных данных:', err);
       setError('Не удалось загрузить отфильтрованные данные');
     } finally {
       setIsLoading(false);
     }
   };

    useEffect(() => {
        loadFilteredData();
      }, [selectedCategoryId, selectedFilterTagIds, searchQuery]);

  const handleEntryAdded = () => {
    loadData();
    setShowAddForm(false);
  };

  // === ФУНКЦИЯ ЭКСПОРТА В CSV (Открывается в Excel) ===
    const handleExport = () => {
      if (entries.length === 0) {
        alert("Нет записей для экспорта");
        return;
      }
  
      // 1. Создаем заголовки колонок
      const headers = ["Дата", "Тип", "Категория", "Тэги", "Заголовок", "Описание", "Решение"];
      
      // 2. Преобразуем данные в строки CSV
      const rows = entries.map((entry: any) => {
        // Форматируем дату
        const date = new Date(entry.createdAt).toLocaleString('ru-RU');
        // Получаем имя категории или прочерк
        const category = entry.category ? entry.category.name : "Без категории";
        // Собираем имена тэгов через запятую
        const tags = entry.tags && entry.tags.length > 0 
          ? entry.tags.map((t: any) => t.name).join(", ") 
          : "Без тэгов";
        
        // Функция для экранирования кавычек и переносов строк в CSV
        const escapeCsv = (text: string) => `"${String(text || "").replace(/"/g, '""').replace(/\n/g, ' ')}"`;
  
        return [
          escapeCsv(date),
          escapeCsv(entry.type === 'Problem' ? 'Проблема' : 'Событие'),
          escapeCsv(category),
          escapeCsv(tags),
          escapeCsv(entry.title),
          escapeCsv(entry.description),
          escapeCsv(entry.solution || "")
        ].join(",");
      });
  
      // 3. Собираем всё в один текст с разделением строк
      // Добавляем BOM (\uFEFF), чтобы Excel корректно распознавал кириллицу (UTF-8)
      const csvContent = "\uFEFF" + [headers.join(","), ...rows].join("\n");
  
      // 4. Создаем файл и инициируем скачивание
      const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.setAttribute("href", url);
      
      // Имя файла с текущей датой
      const fileName = `EngineerLog_Export_${new Date().toISOString().slice(0, 10)}.csv`;
      link.setAttribute("download", fileName);
      
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-900 text-white flex items-center justify-center">
        <div className="text-xl text-gray-400">Загрузка...</div>
      </div>
    );
  }

  if (error || !project) {
    return (
      <div className="min-h-screen bg-gray-900 text-white p-4">
        <div className="max-w-4xl mx-auto">
          <button onClick={onBack} className="mb-4 px-4 py-2 bg-gray-700 hover:bg-gray-600 rounded-lg transition">
            ← Назад к проектам
          </button>
          <div className="p-4 bg-red-900/50 border border-red-500 text-red-200 rounded-lg">
            {error || 'Проект не найден'}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-900 text-white p-4">
      <div className="max-w-4xl mx-auto">
        <div className="mb-8">
          <button onClick={onBack} className="mb-4 px-4 py-2 bg-gray-700 hover:bg-gray-600 rounded-lg transition flex items-center gap-2">
            ← Назад к проектам
          </button>
          <h1 className="text-3xl font-bold text-blue-500 mb-2">{project.name}</h1>
          <p className="text-gray-400">{project.description}</p>
        </div>


<div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-3">
          <h2 className="text-2xl font-bold text-white">Записи дневника</h2>
          <div className="flex gap-3 w-full sm:w-auto">
            <button
              onClick={handleExport}
              disabled={entries.length === 0}
              className="flex-1 sm:flex-none px-4 py-2 bg-green-700 hover:bg-green-600 disabled:bg-gray-700 disabled:text-gray-500 text-white font-medium rounded-lg transition flex items-center justify-center gap-2"
            >
              <span>📥</span> Экспорт в Excel
            </button>
            <button
              onClick={() => setShowAddForm(true)}
              className="flex-1 sm:flex-none px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition flex items-center justify-center gap-2"
            >
              <span>+</span> Новая запись
            </button>
          </div>
        </div>

        {/* === ПАНЕЛЬ ФИЛЬТРОВ === */}
              <div className="mb-6 p-4 bg-gray-800 rounded-lg border border-gray-700">
                <h4 className="text-sm font-semibold text-gray-400 mb-3">Фильтры и поиск</h4>
                
                {/* Поле поиска */}
                <div className="mb-4">
                  <input
                    type="text"
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    placeholder="Поиск по заголовку, описанию или решению..."
                    className="w-full px-4 py-2 bg-gray-700 border border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none text-white placeholder-gray-500"
                  />
                </div>
        
                {/* Фильтр по категории */}
                <div className="mb-4">
                  <label className="block text-xs font-medium text-gray-400 mb-2">Класс (Категория)</label>
                  <select
                    value={selectedCategoryId}
                    onChange={(e) => setSelectedCategoryId(e.target.value === '' ? '' : Number(e.target.value))}
                    className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none text-white text-sm"
                  >
                    <option value="">Все классы</option>
                    {categories.map(cat => (
                      <option key={cat.id} value={cat.id}>{cat.name}</option>
                    ))}
                  </select>
                </div>
        
                {/* Фильтр по тэгам */}
                <div>
                  <label className="block text-xs font-medium text-gray-400 mb-2">Тэги</label>
                  <div className="flex flex-wrap gap-2">
                    {tags.map(tag => (
                      <button
                        key={tag.id}
                        type="button"
                        onClick={() => {
                          setSelectedFilterTagIds(prev =>
                            prev.includes(tag.id) ? prev.filter(id => id !== tag.id) : [...prev, tag.id]
                          );
                        }}
                        className={`px-3 py-1 text-xs rounded-full border transition ${
                          selectedFilterTagIds.includes(tag.id)
                            ? 'bg-blue-600 border-blue-500 text-white'
                            : 'bg-gray-700 border-gray-600 text-gray-300 hover:bg-gray-600'
                        }`}
                      >
                        {tag.name}
                      </button>
                    ))}
                  </div>
                </div>
        
                {/* Кнопка сброса фильтров */}
                {(selectedCategoryId !== '' || selectedFilterTagIds.length > 0 || searchQuery.trim() !== '') && (
                  <button
                    type="button"
                    onClick={() => {
                      setSelectedCategoryId('');
                      setSelectedFilterTagIds([]);
                      setSearchQuery('');
                    }}
                    className="mt-4 px-4 py-2 bg-red-600 hover:bg-red-700 text-white text-sm font-medium rounded-lg transition"
                  >
                    Сбросить фильтры
                  </button>
                )}
              </div>

        {showAddForm && (
          <AddEntryForm
            projectId={projectId}
            onEntryAdded={handleEntryAdded}
            onCancel={() => setShowAddForm(false)}
            categories={categories}
            tags={tags}
          />
        )}

        {entries.length === 0 ? (
          <div className="text-center py-12 text-gray-500 bg-gray-800 rounded-lg border border-gray-700 border-dashed">
            Записей пока нет. Добавьте первую запись!
          </div>
        ) : (
          <div className="space-y-4">
            {entries.map((entry) => {
              const formattedDate = new Date(entry.createdAt).toLocaleDateString('ru-RU', {
                day: 'numeric', month: 'long', year: 'numeric', hour: '2-digit', minute: '2-digit'
              });

              const isProblem = entry.type === 'Problem';
              const badgeClass = isProblem
                ? 'bg-red-900/50 text-red-300 border-red-700'
                : 'bg-blue-900/50 text-blue-300 border-blue-700';
              const badgeText = isProblem ? 'Проблема' : 'Событие';

              return (
                <div key={entry.id} className="p-6 bg-gray-800 rounded-lg shadow-md border border-gray-700 hover:border-gray-600 transition">
                  <div className="flex justify-between items-start mb-3">
                    <div>
                      <h3 className="text-lg font-semibold text-white mb-1">{entry.title}</h3>
                      <div className="text-sm text-gray-500">{formattedDate}</div>
                    </div>
                    <span className={`px-3 py-1 text-sm rounded-full border ${badgeClass}`}>
                      {badgeText}
                    </span>
                  </div>
                  {/* === БЕЙДЖИ КАТЕГОРИИ И ТЭГОВ === */}
                                    <div className="flex flex-wrap gap-2 mt-3 mb-3">
                                      {/* Бейдж категории */}
                                      {entry.category && (
                                        <span 
                                          className="px-3 py-1 text-xs font-semibold rounded-full"
                                          style={{ backgroundColor: entry.category.color + '20', color: entry.category.color, border: `1px solid ${entry.category.color}` }}
                                        >
                                          {entry.category.name}
                                        </span>
                                      )}
                                      
                                      {/* Бейджи тэгов */}
                                      {entry.tags && entry.tags.map((tag: any) => (
                                        <span 
                                          key={tag.id}
                                          className="px-2 py-1 text-xs rounded-full"
                                          style={{ backgroundColor: tag.color + '20', color: tag.color, border: `1px solid ${tag.color}` }}
                                        >
                                          {tag.name}
                                        </span>
                                      ))}
                                    </div>
                  <p className="text-gray-300 whitespace-pre-wrap leading-relaxed">{entry.description}</p>

                  {/* Отображаем решение, только если оно есть */}
                  {entry.solution && (
                    <div className="mt-4 pt-4 border-t border-gray-700">
                      <span className="text-sm font-medium text-green-400 block mb-1">Решение:</span>
                      <p className="text-gray-400 whitespace-pre-wrap">{entry.solution}</p>
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}

// === Компонент формы добавления записи (Обновленный с Категориями и Тэгами) ===
interface AddEntryFormProps {
  projectId: number;
  onEntryAdded: () => void;
  onCancel: () => void;
  categories: any[];
  tags: any[];
}

function AddEntryForm({ projectId, onEntryAdded, onCancel, categories, tags }: AddEntryFormProps) {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [solution, setSolution] = useState('');
  const [type, setType] = useState<'Event' | 'Problem'>('Problem'); // По умолчанию Проблема, так как чаще нужно решение
  const [categoryId, setCategoryId] = useState<number | ''>('');
  const [selectedTagIds, setSelectedTagIds] = useState<number[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const toggleTag = (tagId: number) => {
    setSelectedTagIds(prev => 
      prev.includes(tagId) ? prev.filter(id => id !== tagId) : [...prev, tagId]
    );
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      await apiRequest('/logentries', {
        method: 'POST',
        body: JSON.stringify({
          projectId,
          categoryId: categoryId === '' ? null : Number(categoryId),
          tagIds: selectedTagIds,
          type,
          title,
          description,
          solution: solution.trim() === '' ? null : solution
        }),
      });

      onEntryAdded();
    } catch (err: any) {
      setError(err.message || 'Не удалось создать запись');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="mb-6 p-6 bg-gray-800 rounded-lg border border-gray-700 shadow-xl">
      <h3 className="text-xl font-bold text-blue-500 mb-4">Новая запись</h3>

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-400 mb-2">Заголовок</label>
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            className="w-full px-4 py-3 bg-gray-700 border border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none text-white placeholder-gray-500"
            placeholder="Например: Настройка шпинделя"
            required
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-400 mb-2">Класс (Категория)</label>
            <select
              value={categoryId}
              onChange={(e) => setCategoryId(e.target.value === '' ? '' : Number(e.target.value))}
              className="w-full px-4 py-3 bg-gray-700 border border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none text-white"
            >
              <option value="">-- Не выбрано --</option>
              {categories.map(cat => (
                <option key={cat.id} value={cat.id}>{cat.name}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-400 mb-2">Тип записи</label>
            <select
              value={type}
              onChange={(e) => setType(e.target.value as 'Event' | 'Problem')}
              className="w-full px-4 py-3 bg-gray-700 border border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none text-white"
            >
              <option value="Problem">Проблема / Баг</option>
              <option value="Event">Событие / Фича</option></select>
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-400 mb-2">Тэги (выбери несколько)</label>
          <div className="flex flex-wrap gap-2">
            {tags.map(tag => (
              <button
                key={tag.id}
                type="button"
                onClick={() => toggleTag(tag.id)}
                className={`px-3 py-1 text-sm rounded-full border transition ${
                  selectedTagIds.includes(tag.id)
                    ? 'bg-blue-600 border-blue-500 text-white'
                    : 'bg-gray-700 border-gray-600 text-gray-300 hover:bg-gray-600'
                }`}
              >
                {tag.name}
              </button>
            ))}
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-400 mb-2">Описание</label>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            rows={4}
            className="w-full px-4 py-3 bg-gray-700 border border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none text-white placeholder-gray-500 resize-none"
            placeholder="Опишите, что произошло..."
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-400 mb-2">Решение (необязательно)</label>
          <textarea
            value={solution}
            onChange={(e) => setSolution(e.target.value)}
            rows={3}
            className="w-full px-4 py-3 bg-gray-700 border border-gray-600 rounded-lg focus:ring-2 focus:ring-green-500 outline-none text-white placeholder-gray-500 resize-none"
            placeholder="Как была решена эта проблема?..."
          />
        </div>

        {error && (
          <div className="p-3 bg-red-900/50 border border-red-500 text-red-200 rounded-lg text-sm">
            {error}
          </div>
        )}

        <div className="flex gap-3">
          <button
            type="button"
            onClick={onCancel}
            className="flex-1 py-3 px-4 bg-gray-700 hover:bg-gray-600 text-white font-medium rounded-lg transition"
          >
            Отмена
          </button>
          <button
            type="submit"
            disabled={isLoading}
            className="flex-1 py-3 px-4 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-800 text-white font-semibold rounded-lg transition shadow-lg"
          >
            {isLoading ? 'Сохранение...' : 'Сохранить'}
          </button>
        </div>
      </form>
    </div>
  );
}
