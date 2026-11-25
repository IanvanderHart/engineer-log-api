import { useState, useCallback } from 'react';
import Login from './components/Login';
import Dashboard from './components/Dashboard';
import Toast from './components/Toast';

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(!!localStorage.getItem('token'));
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' | 'info' } | null>(null);

  const handleLoginSuccess = useCallback(() => {
    setIsAuthenticated(true);
    setToast({ message: 'Успешный вход!', type: 'success' });
  }, []);

  const handleLogout = useCallback(() => {
    localStorage.removeItem('token');
    setIsAuthenticated(false);
    setToast({ message: 'Вы вышли из системы', type: 'info' });
  }, []);

  return (
    <>
      {isAuthenticated ? (
        <Dashboard onLogout={handleLogout} />
      ) : (
        <Login onLoginSuccess={handleLoginSuccess} />
      )}

      {toast && (
        <Toast
          message={toast.message}
          type={toast.type}
          onClose={() => setToast(null)}
        />
      )}
    </>
  );
}

export default App;
