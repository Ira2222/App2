# Location CRUD Feature

Complete location management with validation, RFC 7807 error handling, and a professional UI.

## Files

- **types.ts** — TypeScript interfaces matching backend DTOs exactly
  - `LocationDto` — read model (API responses)
  - `LocationUpsertDto` — write model (create/update)
  - `LocationType` — enum for site type
  - `ProblemDetails` — RFC 7807 error structure

- **api.ts** — API client with smart error handling
  - Checks `application/problem+json` content-type
  - Throws user-friendly error messages
  - Handles CORS and network errors gracefully

- **LocationForm.tsx** — Create/edit form with validation
  - Client-side validation matching backend rules
  - Inline error messages
  - Supports create and edit modes
  - Deactivate checkbox for soft deletes

- **LocationList.tsx** — List view with full CRUD
  - Table with sortable columns
  - Edit inline
  - Deactivate (soft delete) action
  - Filter active/inactive locations
  - Empty state with call-to-action

- **styles.css** — Professional UI styling
  - Clean form and table styles
  - Responsive design
  - Accessibility-friendly colors and contrast

## Setup

1. **Configure API URL** (if not localhost:5187):
   ```bash
   cp apps/web/.env.local.example apps/web/.env.local
   # Edit .env.local and set VITE_API_BASE_URL
   ```

2. **Install dependencies**:
   ```bash
   cd apps/web
   pnpm install  # or npm install
   ```

3. **Start the API** (LocationForm):
   ```bash
   cd src/LocationForm.Api
   dotnet run
   # API runs at http://localhost:5187
   ```

4. **Start the web app** (in another terminal):
   ```bash
   cd apps/web
   pnpm dev  # or npm run dev
   # Web runs at http://localhost:5173
   ```

5. **Navigate to**:
   - Locations: http://localhost:5173/
   - Todos (if enabled): http://localhost:5173/todos

## Validation Rules

All rules are enforced both client-side and server-side:

| Field | Rules |
|-------|-------|
| Name | Required, max 160 chars |
| Address Line 1 | Required, max 160 chars |
| Address Line 2 | Optional, max 160 chars |
| City | Required, max 80 chars |
| State | Required, exactly 2 chars (e.g., "MD", "VA") |
| ZIP | Required, format `^\d{5}(-\d{4})?$` (12345 or 12345-6789) |
| Department | Optional, max 100 chars |
| Division | Optional, max 100 chars |
| Section | Optional, max 100 chars |
| Requester Name | Optional, max 100 chars |
| Requester Email | Optional, valid email, max 256 chars |
| Requester Phone | Optional, pattern `^[\d\s\-\(\)\+\.ext]+$`, max 40 chars |
| Use As | Required, enum: ShipToSite / BillToSite / OfficeSite / InternalSite |
| Deactivate | Soft delete checkbox (sets IsActive = false) |

## API Endpoints

All endpoints are at `/api/locations` base path:

- `GET /api/locations?includeInactive=false` — List all active/all locations
- `GET /api/locations/{id}` — Get location by ID
- `POST /api/locations` — Create new location
- `PUT /api/locations/{id}` — Update location
- `DELETE /api/locations/{id}` — Deactivate location (soft delete)

## Troubleshooting

### "Failed to fetch" error

This typically means the browser can't reach the API. Check:

1. **API is running**: `curl http://localhost:5187/healthz/ready`
2. **VITE_API_BASE_URL is set**: Check `apps/web/.env.local`
3. **CORS is enabled**: API should allow `http://localhost:5173`
4. **Network tab**: Check the actual request URL in browser DevTools

### Validation errors on submit

- Client-side validation must pass first (red error messages below each field)
- Server will return RFC 7807 Problem Details on validation failure
- Check the API response in browser console for details

### Form won't save

- Ensure all required fields are filled (marked with *)
- Check that state is exactly 2 characters
- ZIP code must match pattern: 5 digits or 5+4 digits with hyphen

## Features

✓ Full CRUD (Create, Read, Update, Delete)
✓ Soft deletes with IsActive flag
✓ Client-side validation before submit
✓ RFC 7807 Problem Details error handling
✓ Responsive design
✓ Active state in navbar
✓ Loading and error states
✓ Empty state with guidance

## Related Files

- Backend: `src/LocationForm.Api/Endpoints/Locations/LocationsEndpointGroup.cs`
- Domain Model: `src/LocationForm.Domain/Entities/Location.cs`
- Validators: `src/LocationForm.Application/Features/Locations/Validators/LocationUpsertValidator.cs`
