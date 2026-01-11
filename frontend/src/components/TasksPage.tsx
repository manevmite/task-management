import React, { useState } from 'react';
import { useTasks } from '../context/TaskContext';
import TaskForm from './TaskForm';
import TaskFilter from './TaskFilter';
import Pagination from './Pagination';
import DeleteConfirmModal from './DeleteConfirmModal';
import { Task } from '../types';
import { FiPlus, FiEdit2, FiTrash2, FiCheckCircle, FiCircle, FiFilter, FiCheckSquare, FiClock, FiCalendar } from 'react-icons/fi';

const TasksPage: React.FC = () => {
  const {
    tasks,
    loading,
    error,
    deleteTask,
    toggleTaskCompletion,
    page,
    pageSize,
    totalCount,
    totalPages,
    setPage,
    setPageSize,
  } = useTasks();
  const [showForm, setShowForm] = useState<boolean>(false);
  const [editingTask, setEditingTask] = useState<Task | null>(null);
  const [taskToDelete, setTaskToDelete] = useState<Task | null>(null);
  const [isDeleting, setIsDeleting] = useState<boolean>(false);

  const handleAddTask = (): void => {
    setEditingTask(null);
    setShowForm(true);
  };

  const handleEditTask = (task: Task): void => {
    setEditingTask(task);
    setShowForm(true);
  };

  const handleFormClose = (): void => {
    setShowForm(false);
    setEditingTask(null);
  };

  const handleDeleteClick = (task: Task): void => {
    setTaskToDelete(task);
  };

  const handleDeleteConfirm = async (): Promise<void> => {
    if (!taskToDelete) return;

    setIsDeleting(true);
    try {
      await deleteTask(taskToDelete.id);
      setTaskToDelete(null);
    } catch (err) {
      // Error is already handled in TaskContext
      console.error('Error deleting task:', err);
    } finally {
      setIsDeleting(false);
    }
  };

  const handleDeleteCancel = (): void => {
    if (!isDeleting) {
      setTaskToDelete(null);
    }
  };

  const handleToggleCompletion = async (task: Task): Promise<void> => {
    await toggleTaskCompletion(task.id, task.isCompleted);
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 mb-1">Tasks</h1>
          <p className="text-gray-500">Manage all your tasks in one place</p>
        </div>
        <button
          onClick={handleAddTask}
          className="bg-gradient-to-r from-primary-500 to-primary-600 text-white px-5 py-3 rounded-xl font-semibold hover:from-primary-600 hover:to-primary-700 transition-all duration-200 hover:shadow-lg hover:shadow-primary-500/30 hover:-translate-y-0.5 flex items-center gap-2 w-full sm:w-auto justify-center"
        >
          <FiPlus className="text-xl" />
          <span>Add New Task</span>
        </button>
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
        <div className="flex items-center gap-3 mb-3">
          <FiFilter className="text-gray-400 text-lg" />
          <span className="text-sm font-medium text-gray-700">Filter Tasks</span>
        </div>
        <TaskFilter />
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-xl">
          {error}
        </div>
      )}

      {showForm && (
        <TaskForm
          task={editingTask}
          onClose={handleFormClose}
          onSave={handleFormClose}
        />
      )}

      {taskToDelete && (
        <DeleteConfirmModal
          isOpen={!!taskToDelete}
          taskTitle={taskToDelete.title}
          onClose={handleDeleteCancel}
          onConfirm={handleDeleteConfirm}
          isLoading={isDeleting}
        />
      )}

      {loading ? (
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-12 text-center">
          <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mb-4"></div>
          <p className="text-gray-500 font-medium">Loading tasks...</p>
        </div>
      ) : tasks.length === 0 ? (
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-12 text-center">
          <div className="w-20 h-20 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <FiCheckSquare className="text-gray-400 text-3xl" />
          </div>
          <p className="text-gray-900 font-semibold text-lg mb-2">No tasks found</p>
          <p className="text-gray-500 text-sm mb-6">Create your first task to get started!</p>
          <button
            onClick={handleAddTask}
            className="bg-gradient-to-r from-primary-500 to-primary-600 text-white px-6 py-3 rounded-xl font-medium hover:from-primary-600 hover:to-primary-700 transition-all duration-200 inline-flex items-center gap-2"
          >
            <FiPlus />
            <span>Create Task</span>
          </button>
        </div>
      ) : (
        <>
          {/* Mobile Card View */}
          <div className="md:hidden space-y-4">
            {tasks.map((task) => (
              <div
                key={task.id}
                className={`bg-white rounded-xl shadow-sm border-2 transition-all duration-200 hover:shadow-md ${
                  task.isCompleted
                    ? 'border-green-200 bg-green-50/30 opacity-90'
                    : 'border-gray-200 hover:border-primary-300'
                }`}
              >
                <div className="p-4">
                  <div className="flex items-start gap-3">
                    <button
                      onClick={() => handleToggleCompletion(task)}
                      className={`mt-1 flex-shrink-0 w-7 h-7 rounded-lg border-2 flex items-center justify-center transition-all duration-200 cursor-pointer ${
                        task.isCompleted
                          ? 'bg-green-500 border-green-500 text-white hover:bg-green-600'
                          : 'border-gray-300 hover:border-primary-500 hover:bg-primary-50'
                      }`}
                      title={task.isCompleted ? 'Mark as incomplete' : 'Mark as complete'}
                      aria-label={task.isCompleted ? 'Mark as incomplete' : 'Mark as complete'}
                    >
                      {task.isCompleted ? (
                        <FiCheckCircle className="text-base" />
                      ) : (
                        <FiCircle className="text-base text-gray-400" />
                      )}
                    </button>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-start justify-between gap-2 mb-2">
                        <h3
                          className={`font-semibold text-lg flex-1 ${
                            task.isCompleted
                              ? 'text-gray-400 line-through'
                              : 'text-gray-900'
                          }`}
                        >
                          {task.title}
                        </h3>
                        {task.isCompleted && (
                          <span className="flex-shrink-0 px-2 py-1 text-xs font-semibold bg-green-100 text-green-700 rounded-full">
                            Done
                          </span>
                        )}
                      </div>
                      {task.description && (
                        <p className="text-sm text-gray-600 mb-3 line-clamp-2">
                          {task.description}
                        </p>
                      )}
                      <div className="flex flex-wrap items-center gap-3 text-xs text-gray-500 mb-3">
                        <div className="flex items-center gap-1">
                          <FiCalendar className="text-gray-400" />
                          <span>
                            {new Date(task.createdAt).toLocaleDateString('en-US', {
                              month: 'short',
                              day: 'numeric',
                              year: 'numeric'
                            })}
                          </span>
                        </div>
                        {task.updatedAt && (
                          <div className="flex items-center gap-1">
                            <FiClock className="text-gray-400" />
                            <span>
                              Updated {new Date(task.updatedAt).toLocaleDateString('en-US', {
                                month: 'short',
                                day: 'numeric'
                              })}
                            </span>
                          </div>
                        )}
                      </div>
                      <div className="flex items-center gap-2 pt-3 border-t border-gray-100">
                        <button
                          onClick={() => handleEditTask(task)}
                          className="flex-1 flex items-center justify-center gap-2 px-4 py-2 text-sm font-medium text-primary-600 bg-primary-50 rounded-lg hover:bg-primary-100 transition-colors duration-200"
                        >
                          <FiEdit2 className="text-base" />
                          <span>Edit</span>
                        </button>
                        <button
                          onClick={() => handleDeleteClick(task)}
                          className="flex-1 flex items-center justify-center gap-2 px-4 py-2 text-sm font-medium text-red-600 bg-red-50 rounded-lg hover:bg-red-100 transition-colors duration-200"
                        >
                          <FiTrash2 className="text-base" />
                          <span>Delete</span>
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>

          {/* Desktop Table View */}
          <div className="hidden md:block bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-50 border-b border-gray-200">
                  <tr>
                    <th className="px-6 py-4 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">
                      Status
                    </th>
                    <th className="px-6 py-4 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">
                      Title
                    </th>
                    <th className="px-6 py-4 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">
                      Description
                    </th>
                    <th className="px-6 py-4 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider hidden lg:table-cell">
                      Created
                    </th>
                    <th className="px-6 py-4 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider hidden lg:table-cell">
                      Updated
                    </th>
                    <th className="px-6 py-4 text-right text-xs font-semibold text-gray-600 uppercase tracking-wider">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-100">
                  {tasks.map((task) => (
                    <tr
                      key={task.id}
                      className={`hover:bg-gray-50 transition-colors duration-150 ${
                        task.isCompleted ? 'opacity-75' : ''
                      }`}
                    >
                      <td className="px-6 py-4 whitespace-nowrap">
                        <button
                          onClick={() => handleToggleCompletion(task)}
                          className={`w-6 h-6 rounded-lg border-2 flex items-center justify-center transition-all duration-200 cursor-pointer ${
                            task.isCompleted
                              ? 'bg-green-500 border-green-500 text-white hover:bg-green-600'
                              : 'border-gray-300 hover:border-primary-500 hover:bg-primary-50'
                          }`}
                          title={task.isCompleted ? 'Mark as incomplete' : 'Mark as complete'}
                          aria-label={task.isCompleted ? 'Mark as incomplete' : 'Mark as complete'}
                        >
                          {task.isCompleted ? (
                            <FiCheckCircle className="text-sm" />
                          ) : (
                            <FiCircle className="text-sm text-gray-400" />
                          )}
                        </button>
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-2">
                          <div
                            className={`font-semibold ${
                              task.isCompleted
                                ? 'text-gray-400 line-through'
                                : 'text-gray-900'
                            }`}
                          >
                            {task.title}
                          </div>
                          {task.isCompleted && (
                            <span className="px-2 py-0.5 text-xs font-semibold bg-green-100 text-green-700 rounded-full">
                              Done
                            </span>
                          )}
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <div className="text-sm text-gray-600 max-w-md">
                          {task.description ? (
                            <span className="block truncate" title={task.description}>
                              {task.description}
                            </span>
                          ) : (
                            <span className="text-gray-400 italic">No description</span>
                          )}
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 hidden lg:table-cell">
                        <div className="flex items-center gap-1">
                          <FiCalendar className="text-gray-400" />
                          <span>
                            {new Date(task.createdAt).toLocaleDateString('en-US', {
                              month: 'short',
                              day: 'numeric',
                              year: 'numeric'
                            })}
                          </span>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 hidden lg:table-cell">
                        {task.updatedAt ? (
                          <div className="flex items-center gap-1">
                            <FiClock className="text-gray-400" />
                            <span>
                              {new Date(task.updatedAt).toLocaleDateString('en-US', {
                                month: 'short',
                                day: 'numeric',
                                year: 'numeric'
                              })}
                            </span>
                          </div>
                        ) : (
                          <span className="text-gray-300">-</span>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <div className="flex justify-end gap-1">
                          <button
                            onClick={() => handleEditTask(task)}
                            className="p-2 text-gray-600 hover:text-primary-600 hover:bg-primary-50 rounded-lg transition-colors duration-200"
                            title="Edit task"
                            aria-label="Edit task"
                          >
                            <FiEdit2 className="text-lg" />
                          </button>
                          <button
                            onClick={() => handleDeleteClick(task)}
                            className="p-2 text-gray-600 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors duration-200"
                            title="Delete task"
                            aria-label="Delete task"
                          >
                            <FiTrash2 className="text-lg" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            {totalCount > 0 && (
              <Pagination
                currentPage={page}
                totalPages={totalPages}
                pageSize={pageSize}
                totalCount={totalCount}
                onPageChange={setPage}
                onPageSizeChange={setPageSize}
              />
            )}
          </div>

          {/* Pagination for Mobile */}
          {totalCount > 0 && (
            <div className="md:hidden">
              <Pagination
                currentPage={page}
                totalPages={totalPages}
                pageSize={pageSize}
                totalCount={totalCount}
                onPageChange={setPage}
                onPageSizeChange={setPageSize}
              />
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default TasksPage;
