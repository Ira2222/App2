import { useState, type FormEvent } from "react";
import type { LocationDto, LocationUpsertDto, LocationType } from "./types";

interface LocationFormProps {
  location?: LocationDto | null;
  onSubmit: (dto: LocationUpsertDto) => Promise<void>;
  onCancel: () => void;
}

const LOCATION_TYPES: LocationType[] = [
  "ShipToSite",
  "BillToSite",
  "OfficeSite",
  "InternalSite",
];

/**
 * Form component for creating or editing a Location.
 * Includes client-side validation matching LocationUpsertValidator.cs
 */
export default function LocationForm({
  location,
  onSubmit,
  onCancel,
}: LocationFormProps) {
  const isEditing = !!location;

  // Form state
  const [formData, setFormData] = useState<LocationUpsertDto>({
    name: location?.name ?? "",
    addressLine1: location?.addressLine1 ?? "",
    addressLine2: location?.addressLine2 ?? null,
    city: location?.city ?? "",
    state: location?.state ?? "",
    zip: location?.zip ?? "",
    department: location?.department ?? null,
    division: location?.division ?? null,
    section: location?.section ?? null,
    deactivate: location ? !location.isActive : false,
    useAs: location?.useAs ?? "ShipToSite",
    requesterName: location?.requesterName ?? null,
    requesterEmail: location?.requesterEmail ?? null,
    requesterPhone: location?.requesterPhone ?? null,
  });

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);

  /**
   * Client-side validation matching FluentValidation rules
   */
  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    // Name (required, max 160)
    if (!formData.name.trim()) {
      newErrors.name = "Location name is required";
    } else if (formData.name.length > 160) {
      newErrors.name = "Location name must not exceed 160 characters";
    }

    // AddressLine1 (required, max 160)
    if (!formData.addressLine1.trim()) {
      newErrors.addressLine1 = "Address line 1 is required";
    } else if (formData.addressLine1.length > 160) {
      newErrors.addressLine1 = "Address line 1 must not exceed 160 characters";
    }

    // AddressLine2 (optional, max 160)
    if (formData.addressLine2 && formData.addressLine2.length > 160) {
      newErrors.addressLine2 = "Address line 2 must not exceed 160 characters";
    }

    // City (required, max 80)
    if (!formData.city.trim()) {
      newErrors.city = "City is required";
    } else if (formData.city.length > 80) {
      newErrors.city = "City must not exceed 80 characters";
    }

    // State (required, exactly 2 characters)
    if (!formData.state.trim()) {
      newErrors.state = "State is required";
    } else if (formData.state.length !== 2) {
      newErrors.state = "State must be exactly 2 characters (e.g., 'MD', 'VA')";
    }

    // ZIP (required, format validation)
    if (!formData.zip.trim()) {
      newErrors.zip = "ZIP code is required";
    } else if (formData.zip.length > 10) {
      newErrors.zip = "ZIP code must not exceed 10 characters";
    } else if (!/^\d{5}(-\d{4})?$/.test(formData.zip)) {
      newErrors.zip = "ZIP code must be in format 12345 or 12345-6789";
    }

    // Department (optional, max 100)
    if (formData.department && formData.department.length > 100) {
      newErrors.department = "Department must not exceed 100 characters";
    }

    // Division (optional, max 100)
    if (formData.division && formData.division.length > 100) {
      newErrors.division = "Division must not exceed 100 characters";
    }

    // Section (optional, max 100)
    if (formData.section && formData.section.length > 100) {
      newErrors.section = "Section must not exceed 100 characters";
    }

    // RequesterName (optional, max 100)
    if (formData.requesterName && formData.requesterName.length > 100) {
      newErrors.requesterName = "Requester name must not exceed 100 characters";
    }

    // RequesterEmail (optional, email format, max 256)
    if (formData.requesterEmail) {
      if (formData.requesterEmail.length > 256) {
        newErrors.requesterEmail = "Requester email must not exceed 256 characters";
      } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.requesterEmail)) {
        newErrors.requesterEmail = "Requester email must be a valid email address";
      }
    }

    // RequesterPhone (optional, max 40, pattern validation)
    if (formData.requesterPhone) {
      if (formData.requesterPhone.length > 40) {
        newErrors.requesterPhone = "Requester phone must not exceed 40 characters";
      } else if (!/^[\d\s\-\(\)\+\.ext]+$/.test(formData.requesterPhone)) {
        newErrors.requesterPhone = "Requester phone contains invalid characters";
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    setSubmitting(true);
    try {
      await onSubmit(formData);
    } catch (err) {
      alert(err instanceof Error ? err.message : "An error occurred");
    } finally {
      setSubmitting(false);
    }
  };

  const handleChange = (field: keyof LocationUpsertDto, value: unknown) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    // Clear error for this field when user starts typing
    if (errors[field]) {
      setErrors((prev) => {
        const newErrors = { ...prev };
        delete newErrors[field];
        return newErrors;
      });
    }
  };

  return (
    <form onSubmit={handleSubmit} className="location-form">
      <h2>{isEditing ? "Edit Location" : "New Location"}</h2>

      {/* Basic Information */}
      <fieldset>
        <legend>Basic Information</legend>

        <div className="form-field">
          <label htmlFor="name">Location Name *</label>
          <input
            id="name"
            type="text"
            value={formData.name}
            onChange={(e) => handleChange("name", e.target.value)}
            maxLength={160}
            required
          />
          {errors.name && <span className="error">{errors.name}</span>}
        </div>

        <div className="form-field">
          <label htmlFor="useAs">Use As *</label>
          <select
            id="useAs"
            value={formData.useAs}
            onChange={(e) => handleChange("useAs", e.target.value as LocationType)}
            required
          >
            {LOCATION_TYPES.map((type) => (
              <option key={type} value={type}>
                {type.replace(/([A-Z])/g, " $1").trim()}
              </option>
            ))}
          </select>
          {errors.useAs && <span className="error">{errors.useAs}</span>}
        </div>
      </fieldset>

      {/* Address */}
      <fieldset>
        <legend>Address</legend>

        <div className="form-field">
          <label htmlFor="addressLine1">Address Line 1 *</label>
          <input
            id="addressLine1"
            type="text"
            value={formData.addressLine1}
            onChange={(e) => handleChange("addressLine1", e.target.value)}
            maxLength={160}
            required
          />
          {errors.addressLine1 && <span className="error">{errors.addressLine1}</span>}
        </div>

        <div className="form-field">
          <label htmlFor="addressLine2">Address Line 2</label>
          <input
            id="addressLine2"
            type="text"
            value={formData.addressLine2 ?? ""}
            onChange={(e) =>
              handleChange("addressLine2", e.target.value || null)
            }
            maxLength={160}
          />
          {errors.addressLine2 && <span className="error">{errors.addressLine2}</span>}
        </div>

        <div className="form-row">
          <div className="form-field">
            <label htmlFor="city">City *</label>
            <input
              id="city"
              type="text"
              value={formData.city}
              onChange={(e) => handleChange("city", e.target.value)}
              maxLength={80}
              required
            />
            {errors.city && <span className="error">{errors.city}</span>}
          </div>

          <div className="form-field">
            <label htmlFor="state">State *</label>
            <input
              id="state"
              type="text"
              value={formData.state}
              onChange={(e) =>
                handleChange("state", e.target.value.toUpperCase())
              }
              maxLength={2}
              placeholder="MD"
              required
            />
            {errors.state && <span className="error">{errors.state}</span>}
          </div>

          <div className="form-field">
            <label htmlFor="zip">ZIP Code *</label>
            <input
              id="zip"
              type="text"
              value={formData.zip}
              onChange={(e) => handleChange("zip", e.target.value)}
              maxLength={10}
              placeholder="12345"
              required
            />
            {errors.zip && <span className="error">{errors.zip}</span>}
          </div>
        </div>
      </fieldset>

      {/* Organization */}
      <fieldset>
        <legend>Organization</legend>

        <div className="form-field">
          <label htmlFor="department">Department</label>
          <input
            id="department"
            type="text"
            value={formData.department ?? ""}
            onChange={(e) =>
              handleChange("department", e.target.value || null)
            }
            maxLength={100}
          />
          {errors.department && <span className="error">{errors.department}</span>}
        </div>

        <div className="form-field">
          <label htmlFor="division">Division</label>
          <input
            id="division"
            type="text"
            value={formData.division ?? ""}
            onChange={(e) => handleChange("division", e.target.value || null)}
            maxLength={100}
          />
          {errors.division && <span className="error">{errors.division}</span>}
        </div>

        <div className="form-field">
          <label htmlFor="section">Section</label>
          <input
            id="section"
            type="text"
            value={formData.section ?? ""}
            onChange={(e) => handleChange("section", e.target.value || null)}
            maxLength={100}
          />
          {errors.section && <span className="error">{errors.section}</span>}
        </div>
      </fieldset>

      {/* Requester */}
      <fieldset>
        <legend>Requester Information</legend>

        <div className="form-field">
          <label htmlFor="requesterName">Requester Name</label>
          <input
            id="requesterName"
            type="text"
            value={formData.requesterName ?? ""}
            onChange={(e) =>
              handleChange("requesterName", e.target.value || null)
            }
            maxLength={100}
          />
          {errors.requesterName && (
            <span className="error">{errors.requesterName}</span>
          )}
        </div>

        <div className="form-field">
          <label htmlFor="requesterEmail">Requester Email</label>
          <input
            id="requesterEmail"
            type="email"
            value={formData.requesterEmail ?? ""}
            onChange={(e) =>
              handleChange("requesterEmail", e.target.value || null)
            }
            maxLength={256}
          />
          {errors.requesterEmail && (
            <span className="error">{errors.requesterEmail}</span>
          )}
        </div>

        <div className="form-field">
          <label htmlFor="requesterPhone">Requester Phone</label>
          <input
            id="requesterPhone"
            type="tel"
            value={formData.requesterPhone ?? ""}
            onChange={(e) =>
              handleChange("requesterPhone", e.target.value || null)
            }
            maxLength={40}
            placeholder="(123) 456-7890"
          />
          {errors.requesterPhone && (
            <span className="error">{errors.requesterPhone}</span>
          )}
        </div>
      </fieldset>

      {/* Status */}
      {isEditing && (
        <fieldset>
          <legend>Status</legend>
          <div className="form-field checkbox">
            <label>
              <input
                type="checkbox"
                checked={formData.deactivate}
                onChange={(e) => handleChange("deactivate", e.target.checked)}
              />
              Deactivate this location
            </label>
          </div>
        </fieldset>
      )}

      {/* Actions */}
      <div className="form-actions">
        <button type="submit" disabled={submitting}>
          {submitting ? "Saving..." : isEditing ? "Update" : "Create"}
        </button>
        <button type="button" onClick={onCancel} disabled={submitting}>
          Cancel
        </button>
      </div>
    </form>
  );
}
