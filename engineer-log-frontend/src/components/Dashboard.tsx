import { useState, useEffect } from 'react';
import { apiRequest } from '../lib/api';
import CreateProjectModal from './CreateProjectModal';
import DeleteConfirmModal from './DeleteConfirmModal';
import EditProjectModal from './EditProjectModal';
import ProjectDetail from './ProjectDetail'; // <--- ДОБАВИЛИ ИМПОРТ

// Описываем тип нашего проекта (как в C#)
interface Project {
  id: number;
  name: string;
  description: string;
  entries: any[];
}

interface DashboardProps {
  onLogout: () => void;
}

export default function Dashboard({ onLogout }: DashboardProps) {
  const [projects, setProjects] = useState<Project[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [deleteModal, setDeleteModal] = useState<{ isOpen: boolean; project: Project | null }>({ isOpen: false, project: null });
  const [editModal, setEditModal] = useState<{ isOpen: boolean; project: Project | null }>({ isOpen: false, project: null });
  
  // Состояние для выбранного проекта (null = показываем список, число = показываем детали)
  const [selectedProjectId, setSelectedProjectId] = useState<number | null>(null);

  useEffect(() => {
    loadProjects();
  }, []);

  const loadProjects = async () => {
    setIsLoading(true);
    setError('');
    try {
      const data = await apiRequest('/projects');
      setProjects(data);
    } catch (err: any) {
      setError(err.message || 'Не удалось загрузить проекты');
      if (err.message.includes('401')) {
        localStorage.removeItem('token');
        window.location.reload();
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleProjectCreated = () => {
    loadProjects();
  };

  const handleDelete = async () => {
    if (!deleteModal.project) return;
    try {
      await apiRequest(`/projects/${deleteModal.project.id}`, { method: 'DELETE' });
      setDeleteModal({ isOpen: false, project: null });
      loadProjects();
    } catch (err: any) {
      console.error('Ошибка удаления:', err);
      setDeleteModal({ isOpen: false, project: null });
      alert(err.message || 'Не удалось удалить проект');
    }
  };

  const handleProjectUpdated = () => {
    loadProjects();
  };

  // === ГЛАВНАЯ МАГИЯ: Условный рендеринг ===
  // Если проект выбран, показываем его детали. Иначе показываем список.
  if (selectedProjectId) {
    return (
      <ProjectDetail 
        projectId={selectedProjectId} 
        onBack={() => setSelectedProjectId(null)} 
      />
    );
  }

  return (
    <div className="min-h-screen bg-gray-900 text-white p-4">
      {/* Верхняя панель */}
      <div className="max-w-4xl mx-auto flex justify-between items-center mb-8 p-4 bg-gray-800 rounded-lg shadow-lg border border-gray-700">
        <h1 className="text-2xl font-bold text-blue-500">🛠 Engineer Log</h1>
        <div className="flex gap-3">
          <button
            onClick={() => setIsModalOpen(true)}
            className="px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg transition font-medium flex items-center gap-2"
          >
            <span className="text-xl">+</span> Новый проект
          </button>
          <button
            onClick={onLogout}
            className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg transition font-medium"
          >
            Выйти
          </button>
        </div>
      </div>

      {/* Основной контент: Список проектов */}
      <div className="max-w-4xl mx-auto">
        <h2 className="text-xl font-semibold mb-4 text-gray-300">Мои проекты</h2>

        {isLoading ? (
          <div className="text-center py-12 text-gray-400">Загрузка проектов...</div>
        ) : error ? (
          <div className="p-4 bg-red-900/50 border border-red-500 text-red-200 rounded-lg">
            {error}
            <button onClick={loadProjects} className="ml-4 text-blue-400 hover:underline">Повторить</button>
          </div>
        ) : projects.length === 0 ? (
          <div className="text-center py-12 text-gray-500 bg-gray-800 rounded-lg border border-gray-700 border-dashed">
            Проектов пока нет. Самое время создать первый!
          </div>
        ) : (
          <div className="grid gap-4">
            {projects.map((project) => (
              <div
                key={project.id}
                className="p-6 bg-gray-800 rounded-lg shadow-md border border-gray-700 hover:border-blue-500 transition cursor-pointer"
                onClick={() => setSelectedProjectId(project.id)} // Клик по карточке открывает детали
              >
                <div className="flex justify-between items-start">
                  <div className="flex-1">
                    <h3 className="text-xl font-bold text-white mb-2">{project.name}</h3>
                    <p className="text-gray-400">{project.description}</p>
                  </div>
                  <div className="flex gap-2 ml-4">
                    <button
                      onClick={(e) => {
                        e.stopPropagation(); // Останавливаем всплытие, чтобы не сработал клик по карточке
                        setEditModal({ isOpen: true, project });
                      }}
                      className="px-3 py-1 bg-blue-600 hover:bg-blue-700 text-white text-sm rounded transition"
                    >
                      Изменить
                    </button>
                    <button
                      onClick={(e) => {
                        e.stopPropagation(); // Останавливаем всплытие
                        setDeleteModal({ isOpen: true, project });
                      }}
                      className="px-3 py-1 bg-red-600 hover:bg-red-700 text-white text-sm rounded transition"
                    >
                      Удалить
                    </button>
                  </div>
                </div>
                <div className="mt-4 pt-4 border-t border-gray-700">
                  <span className="px-3 py-1 bg-blue-900/50 text-blue-300 text-sm rounded-full border border-blue-700">
                    {project.entries.length} записей
                  </span>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Модальные окна */}
      <CreateProjectModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onProjectCreated={handleProjectCreated}
      />
      <DeleteConfirmModal
        isOpen={deleteModal.isOpen}
        projectName={deleteModal.project?.name || ''}
        onConfirm={handleDelete}
        onCancel={() => setDeleteModal({ isOpen: false, project: null })}
      />
      <EditProjectModal
        isOpen={editModal.isOpen}
        project={editModal.project}
        onClose={() => setEditModal({ isOpen: false, project: null })}
        onProjectUpdated={handleProjectUpdated}
      />
    </div>
  );
}
