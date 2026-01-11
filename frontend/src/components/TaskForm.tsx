import React, { useState, useEffect } from 'react';
import { useTasks } from '../context/TaskContext';
import { Task, CreateTaskRequest, UpdateTaskRequest } from '../types';
import { FiX, FiSave, FiLoader } from 'react-icons/fi';

interface TaskFormProps {
  task: Task | null;
  onClose: () => void;
  onSave: () => void;
}

const TaskForm: React.FC<TaskFormProps> = ({ task, onClose, onSave }) => {
  const { createTask, updateTask } = useTasks();
  const [title, setTitle] = useState<string>('');
  const [description, setDescription] = useState<string>('');
  const [isCompleted, setIsCompleted] = useState<boolean>(false);
  const [error, setError] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);

  useEffect(() => {
    if (task) {
      setTitle(task.title);
      setDescription(task.description || '');
      setIsCompleted(task.isCompleted);
    } else {
      setTitle('');
      setDescription('');
      setIsCompleted(false);
    }
  }, [task]);

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>): Promise<void> => {
    e.preventDefault();
    setError('');

    if (!title.trim()) {
      setError('Title is required');
      return;
    }

    setLoading(true);
    try {
      if (task) {
        await updateTask(task.id, {
          title: title.trim(),
          description: description.trim(),
          isCompleted,
        });
      } else {
        await createTask({
          title: title.trim(),
          description: description.trim() || undefined,
        });
      }
      onSave();
    } catch (err: any) {
      setError(err.message || 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      className="fixed inset-0 bg-black bg-opacity-60 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-fadeIn"
      onClick={onClose}
    >
      <div
        className="bg-white rounded-2xl w-full max-w-lg max-h-[90vh] overflow-y-auto shadow-2xl border border-gray-100 animate-slideUp"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-5 flex justify-between items-center z-10">
          <h2 className="text-2xl font-bold text-gray-900">
            {task ? 'Edit Task' : 'Create New Task'}
          </h2>
          <button
            onClick={onClose}
            className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors duration-200"
            aria-label="Close"
          >
            <FiX className="text-xl" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6">
          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-xl mb-5 flex items-center gap-2">
              <div className="w-2 h-2 bg-red-500 rounded-full"></div>
              <span className="text-sm font-medium">{error}</span>
            </div>
          )}

          <div className="mb-6">
            <label htmlFor="title" className="block mb-2 text-sm font-semibold text-gray-700">
              Title <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              id="title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="Enter task title"
              maxLength={200}
              required
              autoFocus
              className="w-full px-4 py-3 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all duration-200 bg-gray-50 hover:bg-white"
            />
          </div>

          <div className="mb-6">
            <label htmlFor="description" className="block mb-2 text-sm font-semibold text-gray-700">
              Description
            </label>
            <textarea
              id="description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Enter task description (optional)"
              rows={4}
              maxLength={1000}
              className="w-full px-4 py-3 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent resize-y min-h-[100px] transition-all duration-200 bg-gray-50 hover:bg-white"
            />
            <p className="text-xs text-gray-400 mt-2">{description.length}/1000 characters</p>
          </div>

          {task && (
            <div className="mb-6">
              <label className="flex items-center gap-3 cursor-pointer group p-4 bg-gray-50 rounded-xl hover:bg-gray-100 transition-colors duration-200">
                <input
                  type="checkbox"
                  checked={isCompleted}
                  onChange={(e) => setIsCompleted(e.target.checked)}
                  className="w-5 h-5 cursor-pointer accent-primary-500 rounded border-gray-300 focus:ring-2 focus:ring-primary-500"
                />
                <span className="text-gray-700 font-medium group-hover:text-gray-900">Mark as completed</span>
              </label>
            </div>
          )}

          <div className="flex justify-end gap-3 mt-8 pt-6 border-t border-gray-200">
            <button
              type="button"
              onClick={onClose}
              className="px-6 py-3 rounded-xl font-semibold text-gray-700 bg-gray-100 hover:bg-gray-200 transition-all duration-200"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={loading}
              className="px-6 py-3 rounded-xl font-semibold text-white bg-gradient-to-r from-primary-500 to-primary-600 hover:from-primary-600 hover:to-primary-700 transition-all duration-200 disabled:opacity-60 disabled:cursor-not-allowed flex items-center gap-2 shadow-lg shadow-primary-500/30 hover:shadow-xl"
            >
              {loading ? (
                <>
                  <FiLoader className="animate-spin" />
                  <span>Saving...</span>
                </>
              ) : (
                <>
                  <FiSave />
                  <span>{task ? 'Update Task' : 'Create Task'}</span>
                </>
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default TaskForm;
