import type { LocationDto, LocationUpsertDto, ProblemDetails } from "./types";

const API_BASE = import.meta.env.VITE_API_BASE_URL || "http://localhost:5187";

/**
 * Generic HTTP helper with RFC 7807 Problem Details error handling
 */
async function http<T>(url: string, init?: RequestInit): Promise<T> {
  const res = await fetch(url, init);

  if (!res.ok) {
    const ct = res.headers.get("content-type") ?? "";
    if (ct.includes("application/problem+json")) {
      const problem = (await res.json()) as ProblemDetails;
      const msg =
        problem.detail || problem.title || `HTTP ${res.status} ${res.statusText}`;
      throw new Error(msg);
    }
    // Fallback for non-Problem Details errors
    let fallbackMsg = `HTTP ${res.status} ${res.statusText}`;
    try {
      const text = await res.text();
      if (text) fallbackMsg += `: ${text}`;
    } catch {
      // Ignore parse errors
    }
    throw new Error(fallbackMsg);
  }

  // Handle 204 No Content
  if (res.status === 204) {
    return undefined as T;
  }

  return (await res.json()) as T;
}

/**
 * Location API client
 */
export const locationApi = {
  /**
   * Get all locations
   * @param includeInactive - Include deactivated locations (default: false)
   */
  async getAll(includeInactive = false): Promise<LocationDto[]> {
    const url = new URL("/api/locations", API_BASE);
    if (includeInactive) {
      url.searchParams.set("includeInactive", "true");
    }
    return http<LocationDto[]>(url.toString());
  },

  /**
   * Get location by ID
   */
  async getById(id: string): Promise<LocationDto | null> {
    try {
      return await http<LocationDto>(`${API_BASE}/api/locations/${id}`);
    } catch (err) {
      if (err instanceof Error && err.message.includes("404")) {
        return null;
      }
      throw err;
    }
  },

  /**
   * Create a new location
   * @returns The ID of the created location
   */
  async create(dto: LocationUpsertDto): Promise<string> {
    const res = await http<{ id: string }>(`${API_BASE}/api/locations`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(dto),
    });
    return res.id;
  },

  /**
   * Update an existing location
   */
  async update(id: string, dto: LocationUpsertDto): Promise<void> {
    await http<void>(`${API_BASE}/api/locations/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(dto),
    });
  },

  /**
   * Deactivate a location (soft delete)
   */
  async deactivate(id: string): Promise<void> {
    await http<void>(`${API_BASE}/api/locations/${id}`, {
      method: "DELETE",
    });
  },
};
