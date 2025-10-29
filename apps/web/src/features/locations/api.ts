import type { LocationDto, LocationUpsertDto } from "./types";

const BASE =
  (import.meta as any).env?.VITE_API_BASE_URL?.toString() ??
  ""; // same-origin fallback

type ProblemDetails = {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
};

async function http<T>(url: string, init?: RequestInit): Promise<T> {
  const res = await fetch(url, init);
  if (!res.ok) {
    const ct = res.headers.get("content-type") ?? "";
    if (ct.includes("application/problem+json")) {
      const problem = (await res.json()) as ProblemDetails;
      const msg =
        problem.detail ||
        problem.title ||
        `HTTP ${res.status} ${res.statusText}`;
      throw new Error(msg);
    }
    const text = await res.text().catch(() => "");
    throw new Error(text || `HTTP ${res.status} ${res.statusText}`);
  }
  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}

// -------- API surface --------

export async function listLocations(includeInactive = false): Promise<LocationDto[]> {
  const qp = includeInactive ? "?includeInactive=true" : "";
  return http<LocationDto[]>(`${BASE}/api/locations${qp}`);
}

export async function getLocation(id: string): Promise<LocationDto> {
  return http<LocationDto>(`${BASE}/api/locations/${id}`);
}

export async function createLocation(dto: LocationUpsertDto): Promise<{ id: string }> {
  return http<{ id: string }>(`${BASE}/api/locations`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(dto),
  });
}

export async function updateLocation(id: string, dto: LocationUpsertDto): Promise<void> {
  await http<void>(`${BASE}/api/locations/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(dto),
  });
}

export async function deactivateLocation(id: string): Promise<void> {
  await http<void>(`${BASE}/api/locations/${id}`, { method: "DELETE" });
}
