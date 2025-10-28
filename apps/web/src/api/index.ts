/**
 * App2 API Client
 *
 * Type-safe client for the App2 API based on OpenAPI specification.
 *
 * @example
 * ```typescript
 * import { createApiClient } from './api';
 *
 * const api = createApiClient({
 *   baseUrl: import.meta.env.VITE_API_BASE_URL
 * });
 *
 * const todos = await api.getTodos();
 * ```
 */

export * from './types';
export * from './client';
