import apiClient from './apiClient';
import { Task, CreateTaskRequest, UpdateTaskRequest, PaginatedResponse } from '../types';

/**
 * Tasks API service
 * Handles all task-related CRUD operations
 */
export const tasksAPI = {
  /**
   * Get all tasks for the authenticated user with pagination
   * @param isCompleted - Optional filter by completion status (null = all, true = completed, false = active)
   * @param page - Page number (default: 1)
   * @param pageSize - Number of items per page (default: 5)
   * @returns Promise with paginated response
   */
  getAll: async (
    isCompleted: boolean | null = null,
    page: number = 1,
    pageSize: number = 5
  ): Promise<PaginatedResponse<Task>> => {
    const params: any = { page, pageSize };
    if (isCompleted !== null) {
      params.isCompleted = isCompleted;
    }
    const response = await apiClient.get<PaginatedResponse<Task>>('/tasks', { params });
    return response.data;
  },

  /**
   * Get all tasks for the dashboard (no pagination)
   * @param isCompleted - Optional filter by completion status (null = all, true = completed, false = active)
   * @returns Promise with array of all tasks
   */
  getAllForDashboard: async (isCompleted: boolean | null = null): Promise<Task[]> => {
    const params: any = { page: 1, pageSize: 1000 }; // Large page size to get all tasks
    if (isCompleted !== null) {
      params.isCompleted = isCompleted;
    }
    const response = await apiClient.get<PaginatedResponse<Task>>('/tasks', { params });
    return response.data.items;
  },

  /**
   * Get a specific task by ID
   * @param id - Task ID
   * @returns Promise with task details
   */
  getById: async (id: number): Promise<Task> => {
    const response = await apiClient.get<Task>(`/tasks/${id}`);
    return response.data;
  },

  /**
   * Create a new task
   * @param task - Task creation data (title and optional description)
   * @returns Promise with created task
   */
  create: async (task: CreateTaskRequest): Promise<Task> => {
    const response = await apiClient.post<Task>('/tasks', task);
    return response.data;
  },

  /**
   * Update an existing task
   * @param id - Task ID to update
   * @param task - Updated task data (title, description, isCompleted)
   * @returns Promise with updated task
   */
  update: async (id: number, task: UpdateTaskRequest): Promise<Task> => {
    const response = await apiClient.put<Task>(`/tasks/${id}`, task);
    return response.data;
  },

  /**
   * Delete a task
   * @param id - Task ID to delete
   * @returns Promise that resolves when task is deleted
   */
  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/tasks/${id}`);
  },
};

