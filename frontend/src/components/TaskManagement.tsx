import React, { useState } from 'react';
import Sidebar from './Sidebar';
import Dashboard from './Dashboard';
import TasksPage from './TasksPage';
import { FiMenu, FiLogOut, FiUser } from 'react-icons/fi';

interface TaskManagementProps {
  onLogout: () => void;
}

type ViewType = 'dashboard' | 'tasks';

const TaskManagement: React.FC<TaskManagementProps> = ({ onLogout }) => {
  const [activeView, setActiveView] = useState<ViewType>('dashboard');
  const [sidebarOpen, setSidebarOpen] = useState<boolean>(false);
  const email = localStorage.getItem('email');

  return (
    <div className="min-h-screen flex relative bg-gray-50">
      {/* Mobile sidebar overlay */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 bg-black bg-opacity-60 backdrop-blur-sm z-40 md:hidden transition-opacity duration-300"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Sidebar */}
      <div
        className={`${
          sidebarOpen ? 'translate-x-0' : '-translate-x-full'
        } md:translate-x-0 fixed md:static inset-y-0 left-0 z-50 transition-transform duration-300 ease-in-out`}
      >
        <Sidebar
          activeView={activeView}
          onViewChange={(view) => {
            setActiveView(view);
            setSidebarOpen(false);
          }}
          onClose={() => setSidebarOpen(false)}
        />
      </div>

      <div className="flex-1 flex flex-col w-full md:w-auto">
        <header className="bg-white shadow-sm border-b border-gray-200 sticky top-0 z-30 backdrop-blur-sm bg-white/95">
          <div className="px-4 md:px-6 py-4 flex justify-between items-center">
            <div className="flex items-center gap-4">
              {/* Mobile menu button */}
              <button
                onClick={() => setSidebarOpen(!sidebarOpen)}
                className="md:hidden p-2 rounded-xl text-gray-600 hover:bg-gray-100 hover:text-gray-900 transition-all duration-200"
                aria-label="Toggle menu"
              >
                <FiMenu className="text-2xl" />
              </button>
              <div>
                <h1 className="text-xl md:text-2xl font-bold text-gray-900">
                  {activeView === 'dashboard' ? 'Dashboard' : 'Tasks'}
                </h1>
              </div>
            </div>
            <div className="flex items-center gap-3 md:gap-4">
              <div className="hidden sm:flex items-center gap-2 px-3 py-2 bg-gray-100 rounded-lg">
                <FiUser className="text-gray-600" />
                <span className="text-sm font-medium text-gray-700">{email}</span>
              </div>
              <button
                onClick={onLogout}
                className="flex items-center gap-2 px-4 py-2.5 rounded-xl text-sm md:text-base font-semibold text-gray-700 bg-gray-100 hover:bg-gray-200 transition-all duration-200"
              >
                <FiLogOut />
                <span className="hidden sm:inline">Logout</span>
              </button>
            </div>
          </div>
        </header>

        <main className="flex-1 p-4 md:p-6 overflow-auto">
          {activeView === 'dashboard' ? <Dashboard /> : <TasksPage />}
        </main>
      </div>
    </div>
  );
};

export default TaskManagement;

