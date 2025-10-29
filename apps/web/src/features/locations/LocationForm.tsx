import { useEffect, useMemo, useState } from "react";
import type { LocationUpsertDto, LocationType } from "./types";
import "./styles.css";

const TYPES: LocationType[] = [
  "ShipToSite",
  "BillToSite",
  "OfficeSite",
  "InternalSite",
];

type Props = {
  initial?: Partial<LocationUpsertDto> | null;
  onSubmit: (dto: LocationUpsertDto) => Promise<void> | void;
  onCancel?: () => void;
};

type Errors = Partial<Record<keyof LocationUpsertDto, string>>;

export default function LocationForm({ initial, onSubmit, onCancel }: Props) {
  const [dto, setDto] = useState<LocationUpsertDto>({
    name: "",
    addressLine1: "",
    addressLine2: "",
    city: "",
    state: "MD",
    zip: "",
    department: "",
    division: "",
    section: "",
    deactivate: false,
    useAs: "ShipToSite",
    requesterName: "",
    requesterEmail: "",
    requesterPhone: "",
    ...(initial ?? {}),
  });

  useEffect(() => {
    if (initial) setDto((d) => ({ ...d, ...initial }));
  }, [initial]);

  const errors: Errors = useMemo(() => validate(dto), [dto]);

  function validate(x: LocationUpsertDto): Errors {
    const e: Errors = {};
    if (!x.name?.trim()) e.name = "Name is required";
    if (!x.addressLine1?.trim()) e.addressLine1 = "Address is required";
    if (!x.city?.trim()) e.city = "City is required";
    if (!x.state?.trim()) e.state = "State is required";
    if (x.state && x.state.trim().length !== 2) e.state = "Use 2-letter code";
    if (!x.zip?.trim()) e.zip = "ZIP is required";
    if (x.requesterEmail && !/^\S+@\S+\.\S+$/.test(x.requesterEmail))
      e.requesterEmail = "Invalid email";
    return e;
  }

  const hasErrors = Object.keys(errors).length > 0;

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (hasErrors) return;
    await onSubmit(dto);
  }

  return (
    <form onSubmit={handleSubmit} style={{ display: "grid", gap: 12 }}>
      <fieldset className="locations-card" style={{ display: "grid", gap: 8 }}>
        <legend className="locations-h2">Location Details</legend>
        <label>
          Name*
          <input
            className="input"
            value={dto.name}
            onChange={(e) => setDto({ ...dto, name: e.target.value })}
          />
          {errors.name && <small className="error">{errors.name}</small>}
        </label>

        <label>
          Department
          <input
            className="input"
            value={dto.department ?? ""}
            onChange={(e) => setDto({ ...dto, department: e.target.value })}
          />
        </label>

        <label>
          Division
          <input
            className="input"
            value={dto.division ?? ""}
            onChange={(e) => setDto({ ...dto, division: e.target.value })}
          />
        </label>

        <label>
          Section
          <input
            className="input"
            value={dto.section ?? ""}
            onChange={(e) => setDto({ ...dto, section: e.target.value })}
          />
        </label>
      </fieldset>

      <fieldset className="locations-card" style={{ display: "grid", gap: 8 }}>
        <legend className="locations-h2">Address</legend>
        <label>
          Address 1*
          <input
            className="input"
            value={dto.addressLine1}
            onChange={(e) => setDto({ ...dto, addressLine1: e.target.value })}
          />
          {errors.addressLine1 && (
            <small className="error">{errors.addressLine1}</small>
          )}
        </label>

        <label>
          Address 2
          <input
            className="input"
            value={dto.addressLine2 ?? ""}
            onChange={(e) => setDto({ ...dto, addressLine2: e.target.value })}
          />
        </label>

        <div style={{ display: "grid", gridTemplateColumns: "1fr 100px 120px", gap: 8 }}>
          <label>
            City*
            <input
              className="input"
              value={dto.city}
              onChange={(e) => setDto({ ...dto, city: e.target.value })}
            />
            {errors.city && <small className="error">{errors.city}</small>}
          </label>

          <label>
            State*
            <input
              className="input"
              maxLength={2}
              value={dto.state}
              onChange={(e) => setDto({ ...dto, state: e.target.value.toUpperCase() })}
            />
            {errors.state && <small className="error">{errors.state}</small>}
          </label>

          <label>
            ZIP*
            <input
              className="input"
              value={dto.zip}
              onChange={(e) => setDto({ ...dto, zip: e.target.value })}
            />
            {errors.zip && <small className="error">{errors.zip}</small>}
          </label>
        </div>
      </fieldset>

      <fieldset className="locations-card" style={{ display: "grid", gap: 8 }}>
        <legend className="locations-h2">Use As</legend>
        <div className="row">
          {TYPES.map((t) => (
            <label key={t} style={{ display: "inline-flex", gap: 6, alignItems: "center" }}>
              <input
                type="radio"
                name="useAs"
                checked={dto.useAs === t}
                onChange={() => setDto({ ...dto, useAs: t })}
              />
              {t.replace(/([A-Z])/g, " $1").trim()}
            </label>
          ))}
        </div>
      </fieldset>

      <label style={{ display: "inline-flex", gap: 8, alignItems: "center" }}>
        <input
          type="checkbox"
          checked={dto.deactivate}
          onChange={(e) => setDto({ ...dto, deactivate: e.target.checked })}
        />
        Deactivate location
      </label>

      <fieldset className="locations-card" style={{ display: "grid", gap: 8 }}>
        <legend className="locations-h2">Requester</legend>
        <label>
          Name
          <input
            className="input"
            value={dto.requesterName ?? ""}
            onChange={(e) => setDto({ ...dto, requesterName: e.target.value })}
          />
        </label>
        <label>
          Email
          <input
            className="input"
            type="email"
            value={dto.requesterEmail ?? ""}
            onChange={(e) => setDto({ ...dto, requesterEmail: e.target.value })}
          />
          {errors.requesterEmail && (
            <small className="error">{errors.requesterEmail}</small>
          )}
        </label>
        <label>
          Phone
          <input
            className="input"
            value={dto.requesterPhone ?? ""}
            onChange={(e) => setDto({ ...dto, requesterPhone: e.target.value })}
          />
        </label>
      </fieldset>

      <div className="row">
        <button className="btn" type="submit" disabled={hasErrors}>
          Save
        </button>
        {onCancel && (
          <button className="btn" type="button" onClick={onCancel}>
            Cancel
          </button>
        )}
      </div>
    </form>
  );
}
