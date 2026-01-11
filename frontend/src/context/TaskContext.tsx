import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { tasksAPI } from '../services/tasksAPI';
import { TaskContextType, Task, CreateTaskRequest, UpdateTaskRequest } from '../types';

const TaskContext = createContext<TaskContextType | undefined>(undefined);

export const useTasks = (): TaskContextType => {
  const context = useContext(TaskContext);
  if (!context) {
    throw new Error('useTasks must be used within a TaskProvider');
  }
  return context;
};

interface TaskProviderProps {
  children: ReactNode;
}

export const TaskProvider: React.FC<TaskProviderProps> = ({ children }) => {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState<boolean | null>(null); // null = all, true = completed, false = incomplete
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(5);
  const [totalCount, setTotalCount] = useState<number>(0);
  const [totalPages, setTotalPages] = useState<number>(0);

  useEffect(() => {
    fetchTasks();
  }, [filter, page, pageSize]);

  const fetchTasks = async (): Promise<void> => {
    setLoading(true);
    setError(null);
    try {
      const data = await tasksAPI.getAll(filter, page, pageSize);
      // Update state atomically to prevent UI flickering
      setTasks(data.items);
      setTotalCount(data.totalCount);
      setTotalPages(data.totalPages);
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Failed to fetch tasks';
      setError(errorMessage);
      console.error('Error fetching tasks:', err);
      // Preserve existing counts on error to prevent UI flickering
      // Don't reset if we had data before
    } finally {
      setLoading(false);
    }
  };

  const fetchAllTasks = async (): Promise<Task[]> => {
    try {
      const allTasks = await tasksAPI.getAllForDashboard(null);
      return allTasks;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Failed to fetch all tasks';
      setError(errorMessage);
      console.error('Error fetching all tasks:', err);
      return [];
    }
  };

  const createTask = async (task: CreateTaskRequest): Promise<Task> => {
    setLoading(true);
    setError(null);
    try {
      const newTask = await tasksAPI.create(task);
      // Reset to first page after creating a task
      setPage(1);
      await fetchTasks();
      return newTask;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Failed to create task';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const updateTask = async (id: number, task: UpdateTaskRequest): Promise<Task> => {
    setLoading(true);
    setError(null);
    try {
      const updatedTask = await tasksAPI.update(id, task);
      setTasks((prev) =>
        prev.map((t) => (t.id === id ? updatedTask : t))
      );
      return updatedTask;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Failed to update task';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const deleteTask = async (id: number): Promise<void> => {
    setLoading(true);
    setError(null);
    try {
      await tasksAPI.delete(id);
      // If current page becomes empty after deletion, go to previous page
      const remainingTasksOnPage = tasks.filter((t) => t.id !== id).length;
      if (remainingTasksOnPage === 0 && page > 1) {
        setPage(page - 1);
      } else {
        await fetchTasks();
      }
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Failed to delete task';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const toggleTaskCompletion = async (id: number, isCompleted: boolean): Promise<void> => {
    const task = tasks.find((t) => t.id === id);
    if (task) {
      await updateTask(id, {
        title: task.title,
        description: task.description || '',
        isCompleted: !isCompleted,
      });
    }
  };

  const handleSetFilter = (newFilter: boolean | null): void => {
    setFilter(newFilter);
    setPage(1); // Reset to first page when filter changes
  };

  const handleSetPage = (newPage: number): void => {
    if (newPage >= 1 && newPage <= totalPages) {
      setPage(newPage);
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  };

  const handleSetPageSize = (newPageSize: number): void => {
    setPageSize(newPageSize);
    setPage(1); // Reset to first page when page size changes
  };

  const value: TaskContextType = {
    tasks,
    loading,
    error,
    filter,
    setFilter: handleSetFilter,
    page,
    pageSize,
    totalCount,
    totalPages,
    hasPreviousPage: page > 1,
    hasNextPage: page < totalPages,
    setPage: handleSetPage,
    setPageSize: handleSetPageSize,
    fetchTasks,
    fetchAllTasks,
    createTask,
    updateTask,
    deleteTask,
    toggleTaskCompletion,
  };

  return <TaskContext.Provider value={value}>{children}</TaskContext.Provider>;
};

