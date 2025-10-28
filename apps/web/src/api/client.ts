/**
 * Type-safe API client for App2
 * Based on OpenAPI specification
 */

import type { TodoDto, CreateTodoRequest, UpdateTodoRequest, ProblemDetails } from './types';

export class ApiError extends Error {
  constructor(
    message: string,
    public status: number,
    public problemDetails?: ProblemDetails
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

export interface ApiClientConfig {
  baseUrl?: string;
  headers?: Record<string, string>;
}

export class ApiClient {
  private baseUrl: string;
  private headers: Record<string, string>;

  constructor(config: ApiClientConfig = {}) {
    this.baseUrl = config.baseUrl || '';
    this.headers = {
      'Accept': 'application/json',
      'Content-Type': 'application/json',
      ...config.headers
    };
  }

  private async request<T>(
    path: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${this.baseUrl}${path}`;
    const response = await fetch(url, {
      ...options,
      headers: {
        ...this.headers,
        ...options.headers
      }
    });

    if (!response.ok) {
      let problemDetails: ProblemDetails | undefined;

      try {
        // Try to parse ProblemDetails from response
        const contentType = response.headers.get('content-type');
        if (contentType?.includes('application/json') || contentType?.includes('application/problem+json')) {
          problemDetails = await response.json();
        }
      } catch {
        // Ignore parse errors
      }

      throw new ApiError(
        problemDetails?.title || problemDetails?.detail || `Request failed with status ${response.status}`,
        response.status,
        problemDetails
      );
    }

    const contentType = response.headers.get('content-type');
    if (contentType?.includes('application/json')) {
      return response.json();
    }

    return {} as T;
  }

  /**
   * Get all todos
   * @returns Array of todos
   */
  async getTodos(): Promise<TodoDto[]> {
    return this.request<TodoDto[]>('/api/todos');
  }

  /**
   * Create a new todo
   * @param todo - The todo to create
   * @returns The created todo
   */
  async createTodo(todo: CreateTodoRequest): Promise<TodoDto> {
    return this.request<TodoDto>('/api/todos', {
      method: 'POST',
      body: JSON.stringify(todo)
    });
  }

  /**
   * Get a todo by ID
   * @param id - The todo ID
   * @returns The todo or null if not found
   */
  async getTodoById(id: number): Promise<TodoDto | null> {
    try {
      return await this.request<TodoDto>(`/api/todos/${id}`);
    } catch (error) {
      if (error instanceof ApiError && error.status === 404) {
        return null;
      }
      throw error;
    }
  }

  /**
   * Update an existing todo
   * @param id - The todo ID
   * @param todo - The updated todo data
   * @returns The updated todo or null if not found
   */
  async updateTodo(id: number, todo: UpdateTodoRequest): Promise<TodoDto | null> {
    try {
      return await this.request<TodoDto>(`/api/todos/${id}`, {
        method: 'PUT',
        body: JSON.stringify(todo)
      });
    } catch (error) {
      if (error instanceof ApiError && error.status === 404) {
        return null;
      }
      throw error;
    }
  }

  /**
   * Delete a todo
   * @param id - The todo ID
   * @returns true if deleted, false if not found
   */
  async deleteTodo(id: number): Promise<boolean> {
    try {
      await this.request<void>(`/api/todos/${id}`, {
        method: 'DELETE'
      });
      return true;
    } catch (error) {
      if (error instanceof ApiError && error.status === 404) {
        return false;
      }
      throw error;
    }
  }
}

/**
 * Create a configured API client instance
 * @param config - Optional configuration
 * @returns Configured API client
 */
export function createApiClient(config?: ApiClientConfig): ApiClient {
  return new ApiClient(config);
}
