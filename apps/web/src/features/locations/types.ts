// Domain-aligned types
export type LocationType =
  | "ShipToSite"
  | "BillToSite"
  | "OfficeSite"
  | "InternalSite";

export interface LocationDto {
  id: string;
  name: string;
  addressLine1: string;
  addressLine2?: string | null;
  city: string;
  state: string; // 2-char
  zip: string;
  department?: string | null;
  division?: string | null;
  section?: string | null;
  isActive: boolean;
  useAs: LocationType;
  requesterName?: string | null;
  requesterEmail?: string | null;
  requesterPhone?: string | null;
  requestedOn: string; // ISO date (yyyy-MM-dd)
}

export interface LocationUpsertDto {
  name: string;
  addressLine1: string;
  addressLine2?: string | null;
  city: string;
  state: string; // 2-char
  zip: string;
  department?: string | null;
  division?: string | null;
  section?: string | null;
  deactivate: boolean; // inverse of IsActive
  useAs: LocationType;
  requesterName?: string | null;
  requesterEmail?: string | null;
  requesterPhone?: string | null;
}
