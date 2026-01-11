import React from 'react';
import { useTasks } from '../context/TaskContext';
import { FiList, FiClock, FiCheckCircle } from 'react-icons/fi';

const TaskFilter: React.FC = () => {
  const { filter, setFilter } = useTasks();

  const filters: Array<{ value: boolean | null; label: string; icon: React.ElementType }> = [
    { value: null, label: 'All Tasks', icon: FiList },
    { value: false, label: 'Active', icon: FiClock },
    { value: true, label: 'Completed', icon: FiCheckCircle },
  ];

  return (
    <div className="flex gap-2 flex-wrap">
      {filters.map((filterOption) => {
        const Icon = filterOption.icon;
        return (
          <button
            key={filterOption.value === null ? 'all' : String(filterOption.value)}
            onClick={() => setFilter(filterOption.value)}
            className={`flex items-center gap-2 px-4 py-2.5 rounded-lg font-medium transition-all duration-200 ${
              filter === filterOption.value
                ? 'bg-gradient-to-r from-primary-500 to-primary-600 text-white shadow-lg shadow-primary-500/30'
                : 'bg-white text-gray-700 border border-gray-200 hover:border-primary-300 hover:text-primary-600 hover:bg-primary-50'
            }`}
          >
            <Icon className="text-lg" />
            <span>{filterOption.label}</span>
          </button>
        );
      })}
    </div>
  );
};

export default TaskFilter;
