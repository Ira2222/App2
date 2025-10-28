/**
 * Generated types from OpenAPI specification
 * DO NOT EDIT MANUALLY - regenerate when API changes
 */

export interface TodoDto {
  id: number;
  title: string;
  description?: string | null;
  isCompleted: boolean;
  createdAt: string;
  completedAt?: string | null;
}

export interface CreateTodoRequest {
  title: string;
  description?: string | null;
}

export interface UpdateTodoRequest {
  title: string;
  description?: string | null;
  isCompleted: boolean;
}

export interface ProblemDetails {
  type?: string | null;
  title?: string | null;
  status?: number | null;
  detail?: string | null;
  instance?: string | null;
  traceId?: string | null;
}
