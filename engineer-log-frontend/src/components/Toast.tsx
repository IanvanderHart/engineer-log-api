import { useEffect, useState } from 'react';

interface ToastProps {
  message: string;
  type?: 'success' | 'error' | 'info';
  duration?: number;
  onClose: () => void;
}

export default function Toast({ message, type = 'success', duration = 2000, onClose }: ToastProps) {
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    // Плавно появляемся
    setIsVisible(true);
    
    // Через duration начинаем исчезать
    const fadeTimer = setTimeout(() => {
      setIsVisible(false);
    }, duration - 300); // 300мс на анимацию исчезновения
    
    // Полностью удаляем компонент
    const removeTimer = setTimeout(onClose, duration);

    return () => {
      clearTimeout(fadeTimer);
      clearTimeout(removeTimer);
    };
  }, [duration, onClose]);

  const bgColor = {
    success: 'bg-green-600',
    error: 'bg-red-600',
    info: 'bg-blue-600',
  }[type];

  return (
    <div 
      className={`fixed top-4 right-4 z-50 px-6 py-3 ${bgColor} text-white rounded-lg shadow-2xl border border-white/20 transition-all duration-300 ${
        isVisible ? 'opacity-100 translate-y-0' : 'opacity-0 -translate-y-2'
      }`}
    >
      <div className="flex items-center gap-2">
        <span className="text-lg">
          {type === 'success' ? '✓' : type === 'error' ? '✕' : 'ℹ'}
        </span>
        <span className="font-medium">{message}</span>
      </div>
    </div>
  );
}
