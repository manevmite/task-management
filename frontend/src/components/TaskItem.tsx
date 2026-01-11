import React from 'react';
import { useTasks } from '../context/TaskContext';
import { Task } from '../types';

interface TaskItemProps {
  task: Task;
  onEdit: (task: Task) => void;
}

const TaskItem: React.FC<TaskItemProps> = ({ task, onEdit }) => {
  const { deleteTask, toggleTaskCompletion } = useTasks();

  const handleToggle = (): void => {
    toggleTaskCompletion(task.id, task.isCompleted);
  };

  const handleDelete = async (): Promise<void> => {
    if (window.confirm('Are you sure you want to delete this task?')) {
      await deleteTask(task.id);
    }
  };

  const handleEdit = (): void => {
    onEdit(task);
  };

  return (
    <div
      className={`bg-white rounded-lg p-5 shadow-md flex justify-between items-start gap-4 transition-all hover:shadow-lg hover:-translate-y-0.5 ${
        task.isCompleted ? 'opacity-70' : ''
      }`}
    >
      <div className="flex gap-4 flex-1">
        <div className="mt-1">
          <input
            type="checkbox"
            checked={task.isCompleted}
            onChange={handleToggle}
            aria-label="Toggle task completion"
            className="w-5 h-5 cursor-pointer accent-primary-500"
          />
        </div>
        <div className="flex-1">
          <h3
            className={`text-lg font-semibold text-gray-800 mb-2 ${
              task.isCompleted ? 'line-through text-gray-500' : ''
            }`}
          >
            {task.title}
          </h3>
          {task.description && (
            <p className="text-gray-600 mb-3 leading-relaxed break-words">{task.description}</p>
          )}
          <div className="flex gap-4 flex-wrap text-xs text-gray-400">
            <span>Created: {new Date(task.createdAt).toLocaleDateString()}</span>
            {task.updatedAt && (
              <span>Updated: {new Date(task.updatedAt).toLocaleDateString()}</span>
            )}
          </div>
        </div>
      </div>
      <div className="flex gap-2 shrink-0">
        <button
          onClick={handleEdit}
          className="text-lg opacity-70 hover:opacity-100 p-2 rounded-lg hover:bg-gray-100 transition-all hover:scale-110"
          title="Edit task"
        >
          ✏️
        </button>
        <button
          onClick={handleDelete}
          className="text-lg opacity-70 hover:opacity-100 p-2 rounded-lg hover:bg-red-50 transition-all hover:scale-110"
          title="Delete task"
        >
          🗑️
        </button>
      </div>
    </div>
  );
};

export default TaskItem;

