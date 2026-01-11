import React, { useState } from 'react';
import { AuthProvider } from './context/AuthContext';
import { TaskProvider } from './context/TaskContext';
import Login from './components/Login';
import TaskManagement from './components/TaskManagement';

const App: React.FC = () => {
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(
    () => localStorage.getItem('token') !== null
  );

  return (
    <AuthProvider>
      <div className="min-h-screen flex flex-col">
        {!isAuthenticated ? (
          <Login onLogin={() => setIsAuthenticated(true)} />
        ) : (
          <TaskProvider>
            <TaskManagement
              onLogout={() => {
                setIsAuthenticated(false);
                localStorage.removeItem('token');
                localStorage.removeItem('email');
              }}
            />
          </TaskProvider>
        )}
      </div>
    </AuthProvider>
  );
};

export default App;

