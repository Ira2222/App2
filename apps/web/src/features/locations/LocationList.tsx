import { useEffect, useState } from "react";
import {
  listLocations,
  createLocation,
  updateLocation,
  deactivateLocation,
  getLocation,
} from "./api";
import type { LocationDto, LocationUpsertDto, LocationType } from "./types";
import LocationForm from "./LocationForm";
import "./styles.css";

export default function LocationList() {
  const [items, setItems] = useState<LocationDto[]>([]);
  const [includeInactive, setIncludeInactive] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function load() {
    setError(null);
    try {
      setItems(await listLocations(includeInactive));
    } catch (e: any) {
      setError(e?.message ?? "Failed to load locations");
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [includeInactive]);

  async function handleSubmit(dto: LocationUpsertDto) {
    setBusy(true);
    setError(null);
    try {
      if (editingId) {
        await updateLocation(editingId, dto);
      } else {
        await createLocation(dto);
      }
      setEditingId(null);
      await load();
    } catch (e: any) {
      setError(e?.message ?? "Save failed");
    } finally {
      setBusy(false);
    }
  }

  async function startEdit(id: string) {
    setError(null);
    try {
      const x = await getLocation(id);
      const dto: LocationUpsertDto = {
        name: x.name,
        addressLine1: x.addressLine1,
        addressLine2: x.addressLine2 ?? "",
        city: x.city,
        state: x.state,
        zip: x.zip,
        department: x.department ?? "",
        division: x.division ?? "",
        section: x.section ?? "",
        deactivate: !x.isActive,
        useAs: x.useAs as LocationType,
        requesterName: x.requesterName ?? "",
        requesterEmail: x.requesterEmail ?? "",
        requesterPhone: x.requesterPhone ?? "",
      };
      setEditingId(id);
      setFormInitial(dto);
    } catch (e: any) {
      setError(e?.message ?? "Failed to load item");
    }
  }

  async function doDeactivate(id: string) {
    if (!confirm("Deactivate this location?")) return;
    setBusy(true);
    setError(null);
    try {
      await deactivateLocation(id);
      await load();
    } catch (e: any) {
      setError(e?.message ?? "Deactivate failed");
    } finally {
      setBusy(false);
    }
  }

  const [formInitial, setFormInitial] = useState<Partial<LocationUpsertDto> | null>(null);

  return (
    <div className="locations-container">
      <h1 className="locations-h1">Locations</h1>

      <div className="row">
        <label style={{ display: "inline-flex", gap: 8, alignItems: "center" }}>
          <input
            type="checkbox"
            checked={includeInactive}
            onChange={(e) => setIncludeInactive(e.target.checked)}
          />
          Show inactive
        </label>
        {busy && <span className="muted">Working…</span>}
        {error && <span style={{ color: "crimson" }}>{error}</span>}
      </div>

      <div className="locations-card">
        <h2 className="locations-h2">{editingId ? "Edit Location" : "Create Location"}</h2>
        <LocationForm
          key={editingId ?? "new"}
          initial={formInitial ?? undefined}
          onSubmit={handleSubmit}
          onCancel={() => {
            setEditingId(null);
            setFormInitial(null);
          }}
        />
      </div>

      <div className="locations-card" style={{ overflowX: "auto" }}>
        <table>
          <thead>
            <tr>
              <th align="left">Name</th>
              <th align="left">Address</th>
              <th align="left">Type</th>
              <th>Active</th>
              <th align="left">Actions</th>
            </tr>
          </thead>
          <tbody>
            {items.map((x) => (
              <tr key={x.id}>
                <td>{x.name}</td>
                <td>
                  {x.addressLine1}
                  {x.addressLine2 ? `, ${x.addressLine2}` : ""}, {x.city}, {x.state} {x.zip}
                </td>
                <td>{x.useAs.replace(/([A-Z])/g, " $1").trim()}</td>
                <td align="center">{x.isActive ? "✅" : "❌"}</td>
                <td>
                  <button className="btn" onClick={() => startEdit(x.id)}>Edit</button>{" "}
                  {x.isActive && <button className="btn" onClick={() => doDeactivate(x.id)}>Deactivate</button>}
                </td>
              </tr>
            ))}
            {items.length === 0 && (
              <tr>
                <td colSpan={5} style={{ padding: 12 }} className="muted">
                  No locations found.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
