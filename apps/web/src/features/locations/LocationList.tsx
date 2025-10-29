import { useState, useEffect } from "react";
import type { LocationDto, LocationUpsertDto } from "./types";
import { locationApi } from "./api";
import LocationForm from "./LocationForm";
import "./styles.css";

/**
 * List view for locations with inline create/edit and CRUD operations
 */
export default function LocationList() {
  const [locations, setLocations] = useState<LocationDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [includeInactive, setIncludeInactive] = useState(false);

  // Form state
  const [showForm, setShowForm] = useState(false);
  const [editingLocation, setEditingLocation] = useState<LocationDto | null>(null);

  /**
   * Load locations from API
   */
  const loadLocations = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await locationApi.getAll(includeInactive);
      setLocations(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load locations");
    } finally {
      setLoading(false);
    }
  };

  // Load on mount and when includeInactive changes
  useEffect(() => {
    loadLocations();
  }, [includeInactive]);

  /**
   * Handle create
   */
  const handleCreate = async (dto: LocationUpsertDto) => {
    await locationApi.create(dto);
    setShowForm(false);
    await loadLocations();
  };

  /**
   * Handle update
   */
  const handleUpdate = async (dto: LocationUpsertDto) => {
    if (!editingLocation) return;
    await locationApi.update(editingLocation.id, dto);
    setEditingLocation(null);
    setShowForm(false);
    await loadLocations();
  };

  /**
   * Handle deactivate (soft delete)
   */
  const handleDeactivate = async (id: string) => {
    if (!confirm("Are you sure you want to deactivate this location?")) return;
    try {
      await locationApi.deactivate(id);
      await loadLocations();
    } catch (err) {
      alert(err instanceof Error ? err.message : "Failed to deactivate location");
    }
  };

  /**
   * Open form for editing
   */
  const handleEdit = (location: LocationDto) => {
    setEditingLocation(location);
    setShowForm(true);
  };

  /**
   * Cancel form
   */
  const handleCancel = () => {
    setShowForm(false);
    setEditingLocation(null);
  };

  /**
   * Open form for creating
   */
  const handleNew = () => {
    setEditingLocation(null);
    setShowForm(true);
  };

  if (loading) {
    return <div className="loading">Loading locations...</div>;
  }

  if (error) {
    return (
      <div className="error">
        <p>Error: {error}</p>
        <button onClick={loadLocations}>Retry</button>
      </div>
    );
  }

  return (
    <div className="location-list-container">
      <header className="list-header">
        <h1>Locations</h1>
        {!showForm && (
          <button onClick={handleNew} className="btn-primary">
            New Location
          </button>
        )}
      </header>

      {showForm ? (
        <LocationForm
          location={editingLocation}
          onSubmit={editingLocation ? handleUpdate : handleCreate}
          onCancel={handleCancel}
        />
      ) : (
        <>
          <div className="list-controls">
            <label>
              <input
                type="checkbox"
                checked={includeInactive}
                onChange={(e) => setIncludeInactive(e.target.checked)}
              />
              Include inactive locations
            </label>
            <span className="count">
              {locations.length} location{locations.length !== 1 ? "s" : ""}
            </span>
          </div>

          {locations.length === 0 ? (
            <div className="empty-state">
              <p>No locations found.</p>
              <button onClick={handleNew}>Create your first location</button>
            </div>
          ) : (
            <table className="locations-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Address</th>
                  <th>City</th>
                  <th>State</th>
                  <th>ZIP</th>
                  <th>Type</th>
                  <th>Department</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {locations.map((location) => (
                  <tr
                    key={location.id}
                    className={!location.isActive ? "inactive" : ""}
                  >
                    <td>
                      <strong>{location.name}</strong>
                    </td>
                    <td>
                      {location.addressLine1}
                      {location.addressLine2 && (
                        <>
                          <br />
                          {location.addressLine2}
                        </>
                      )}
                    </td>
                    <td>{location.city}</td>
                    <td>{location.state}</td>
                    <td>{location.zip}</td>
                    <td>{location.useAs.replace(/([A-Z])/g, " $1").trim()}</td>
                    <td>{location.department || "-"}</td>
                    <td>
                      <span
                        className={`status-badge ${
                          location.isActive ? "active" : "inactive"
                        }`}
                      >
                        {location.isActive ? "Active" : "Inactive"}
                      </span>
                    </td>
                    <td className="actions">
                      <button
                        onClick={() => handleEdit(location)}
                        className="btn-small"
                        title="Edit"
                      >
                        Edit
                      </button>
                      {location.isActive && (
                        <button
                          onClick={() => handleDeactivate(location.id)}
                          className="btn-small btn-danger"
                          title="Deactivate"
                        >
                          Deactivate
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </>
      )}
    </div>
  );
}
