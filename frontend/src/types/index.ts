// Auth types
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  email: string;
}

// Task types
export interface Task {
  id: number;
  title: string;
  description: string | null;
  isCompleted: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface PaginatedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
}

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  isCompleted: boolean;
}

// Context types
export interface AuthContextType {
  register: (email: string, password: string) => Promise<AuthResponse>;
  login: (email: string, password: string) => Promise<AuthResponse>;
  logout: () => void;
  loading: boolean;
  error: string | null;
}

export interface TaskContextType {
  tasks: Task[];
  loading: boolean;
  error: string | null;
  filter: boolean | null;
  setFilter: (filter: boolean | null) => void;
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  setPage: (page: number) => void;
  setPageSize: (pageSize: number) => void;
  fetchTasks: () => Promise<void>;
  fetchAllTasks: () => Promise<Task[]>;
  createTask: (task: CreateTaskRequest) => Promise<Task>;
  updateTask: (id: number, task: UpdateTaskRequest) => Promise<Task>;
  deleteTask: (id: number) => Promise<void>;
  toggleTaskCompletion: (id: number, isCompleted: boolean) => Promise<void>;
}

