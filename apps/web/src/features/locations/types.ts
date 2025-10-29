/**
 * Location type enum - matches LocationForm.Domain.Enums.LocationType
 */
export type LocationType =
  | "ShipToSite"
  | "BillToSite"
  | "OfficeSite"
  | "InternalSite";

/**
 * Read model for Location entity - matches LocationDto.cs
 * Used for API responses and queries.
 */
export interface LocationDto {
  id: string;
  name: string;
  addressLine1: string;
  addressLine2: string | null;
  city: string;
  state: string;
  zip: string;
  department: string | null;
  division: string | null;
  section: string | null;
  isActive: boolean;
  useAs: LocationType;
  requesterName: string | null;
  requesterEmail: string | null;
  requesterPhone: string | null;
  requestedOn: string; // ISO date (yyyy-MM-dd)
  createdAt: string;   // ISO datetime
  createdBy: string | null;
}

/**
 * Write model for creating or updating a Location - matches LocationUpsertDto.cs
 * Uses "Deactivate" checkbox semantics from the form.
 */
export interface LocationUpsertDto {
  name: string;
  addressLine1: string;
  addressLine2: string | null;
  city: string;
  state: string;
  zip: string;
  department: string | null;
  division: string | null;
  section: string | null;
  deactivate: boolean; // True = set IsActive to false
  useAs: LocationType;
  requesterName: string | null;
  requesterEmail: string | null;
  requesterPhone: string | null;
}

/**
 * RFC 7807 Problem Details for HTTP API errors
 */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}
