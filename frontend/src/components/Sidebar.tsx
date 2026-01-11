import React from 'react';
import { FiLayout, FiCheckSquare, FiX } from 'react-icons/fi';

interface SidebarProps {
  activeView: 'dashboard' | 'tasks';
  onViewChange: (view: 'dashboard' | 'tasks') => void;
  onClose?: () => void;
}

const Sidebar: React.FC<SidebarProps> = ({ activeView, onViewChange, onClose }) => {
  const menuItems = [
    { id: 'dashboard' as const, label: 'Dashboard', icon: FiLayout },
    { id: 'tasks' as const, label: 'Tasks', icon: FiCheckSquare },
  ];

  return (
    <aside className="w-64 bg-white text-gray-800 min-h-screen flex flex-col shadow-lg border-r border-gray-200">
      <div className="p-6 border-b border-gray-200">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-gradient-to-br from-primary-500 to-purple-600 rounded-lg flex items-center justify-center shadow-md">
              <FiCheckSquare className="text-white text-xl" />
            </div>
            <h2 className="text-xl font-bold bg-gradient-to-r from-primary-600 to-purple-600 bg-clip-text text-transparent">
              Task Manager
            </h2>
          </div>
          {onClose && (
            <button
              onClick={onClose}
              className="md:hidden p-1.5 rounded-lg text-gray-500 hover:bg-gray-100 hover:text-gray-700 transition-colors"
              aria-label="Close menu"
            >
              <FiX className="text-xl" />
            </button>
          )}
        </div>
      </div>
      
      <nav className="flex-1 p-4">
        <ul className="space-y-1">
          {menuItems.map((item) => {
            const Icon = item.icon;
            return (
              <li key={item.id}>
                <button
                  onClick={() => onViewChange(item.id)}
                  className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl transition-all duration-200 ${
                    activeView === item.id
                      ? 'bg-gradient-to-r from-primary-500 to-primary-600 text-white shadow-lg shadow-primary-500/30'
                      : 'text-gray-700 hover:bg-gray-50 hover:text-primary-600'
                  }`}
                >
                  <Icon className={`text-xl ${activeView === item.id ? '' : 'text-gray-500'}`} />
                  <span className="font-medium">{item.label}</span>
                </button>
              </li>
            );
          })}
        </ul>
      </nav>
      
      <div className="p-4 border-t border-gray-200">
        <p className="text-xs text-gray-400 text-center">
          Task Management System v1.0
        </p>
      </div>
    </aside>
  );
};

export default Sidebar;
