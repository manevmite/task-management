import apiClient from './apiClient';
import { AuthResponse, RegisterRequest, LoginRequest } from '../types';

/**
 * Authentication API service
 * Handles user registration and login operations
 */
export const authAPI = {
  /**
   * Register a new user
   * @param email - User email address
   * @param password - User password (minimum 6 characters)
   * @returns Promise with authentication response containing token and email
   */
  register: async (email: string, password: string): Promise<AuthResponse> => {
    const response = await apiClient.post<AuthResponse>('/auth/register', {
      email,
      password,
    } as RegisterRequest);
    return response.data;
  },

  /**
   * Login an existing user
   * @param email - User email address
   * @param password - User password
   * @returns Promise with authentication response containing token and email
   */
  login: async (email: string, password: string): Promise<AuthResponse> => {
    const response = await apiClient.post<AuthResponse>('/auth/login', {
      email,
      password,
    } as LoginRequest);
    return response.data;
  },
};

