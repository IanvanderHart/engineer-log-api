interface DeleteConfirmModalProps {
  isOpen: boolean;
  projectName: string;
  onConfirm: () => void;
  onCancel: () => void;
}

export default function DeleteConfirmModal({ isOpen, projectName, onConfirm, onCancel }: DeleteConfirmModalProps) {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 backdrop-blur-sm p-4">
      <div className="w-full max-w-md bg-gray-800 rounded-lg shadow-2xl border border-gray-700">
        <div className="p-6">
          <h2 className="text-2xl font-bold text-red-500 mb-4">Подтверждение удаления</h2>
          <p className="text-gray-300 mb-6">
            Вы уверены, что хотите удалить проект <span className="font-semibold text-white">"{projectName}"</span>?
            <br />
            <span className="text-sm text-gray-400">Это действие нельзя отменить.</span>
          </p>

          <div className="flex gap-3">
            <button
              onClick={onCancel}
              className="flex-1 py-3 px-4 bg-gray-700 hover:bg-gray-600 text-white font-medium rounded-lg transition"
            >
              Отмена
            </button>
            <button
              onClick={onConfirm}
              className="flex-1 py-3 px-4 bg-red-600 hover:bg-red-700 text-white font-semibold rounded-lg transition shadow-lg"
            >
              Удалить
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
