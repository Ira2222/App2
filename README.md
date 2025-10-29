# App2 Starter

[![CI](https://github.com/Ira2222/App2/actions/workflows/ci.yml/badge.svg)](https://github.com/Ira2222/App2/actions/workflows/ci.yml)
[![Security](https://github.com/Ira2222/App2/actions/workflows/security.yml/badge.svg)](https://github.com/Ira2222/App2/actions/workflows/security.yml)
[![Container](https://github.com/Ira2222/App2/actions/workflows/container.yml/badge.svg)](https://github.com/Ira2222/App2/actions/workflows/container.yml)
[![Release (provenance)](https://github.com/Ira2222/App2/actions/workflows/release-provenance.yml/badge.svg)](https://github.com/Ira2222/App2/actions/workflows/release-provenance.yml)

A modular .NET 8 Minimal API + Vite React starter built for secure-by-default services and spec-first workflows. The API exposes feature-flagged middleware (auth, CORS, rate limiting, output caching, security headers, observability) and serves as the backend for a React client that consumes generated OpenAPI clients.

---

## 🔖 Template Repository

This is a **template** for building production-ready .NET 8 APIs with React frontends. It includes:

- **Clean Architecture** with CQRS pattern (MediatR)
- **Security**: CodeQL, Trivy scanning, SLSA provenance
- **Multi-arch containers** (linux/amd64, linux/arm64)
- **Feature flags** for auth, CORS, caching, telemetry
- **Optional integrations**: Azure AD, KeyVault, Redis, PostgreSQL
- **Modern frontend**: React 18 + Vite 7 + TypeScript

### Creating a New Project

**Option 1: GitHub UI**
1. Click **"Use this template"** button above
2. Choose **"Create a new repository"**
3. Name your project and create

**Option 2: GitHub CLI**
```bash
gh repo create your-org/your-project --template Ira2222/App2 --private --clone
cd your-project
# Make your changes, then:
git add .
git commit -m "feat: initialize project"
git push
```

**Next Steps After Creating**:
1. Update `appsettings.json` with your Azure AD credentials (if using auth)
2. Rename namespaces from `App2.*` to `YourProject.*` (search/replace across solution)
3. Replace the Todo entity with your domain entities
4. Update `README.md` with your project details
5. Push changes - CI will run automatically

---

## 5-Minute Quickstart

```bash
# 1. Create solution + add projects
cd App2
 dotnet new sln -n App2
 dotnet sln App2.sln add src/App2.*/*.csproj tests/App2.Tests.Integration/App2.Tests.Integration.csproj

# 2. Restore & build
 dotnet restore
 dotnet build

# 3. Install web dependencies
 npm --prefix apps/web install

# 4. Run locally (separate shells)
 dotnet run --project src/App2.Api --launch-profile https
 npm --prefix apps/web run dev
```

The API listens on `https://localhost:5081` by default; adjust launch settings or use `dotnet watch` for hot reload. The web app uses `VITE_API_BASE_URL` to target the API.

## Feature Flags

Toggle middleware via `appsettings.json` → `Features`. Development defaults enable most features while keeping auth disabled until Azure AD values are provided.

| Feature            | Description                                      |
|--------------------|--------------------------------------------------|
| Authentication     | Microsoft Identity Web (JWT bearer)              |
| CORS               | Named allow-list policy                          |
| RateLimiting       | Fixed-window limiter with Retry-After            |
| OutputCaching      | Base + `Todos` policy (tag invalidation)         |
| SecurityHeaders    | NetEscapades API defaults + optional CSP hook    |
| OpenTelemetry      | Adds OTLP exporter if endpoint configured        |

## Dev vs Production

- **Dev auth fallback** (`DevHeader` handler) is only active when `ASPNETCORE_ENVIRONMENT=Development` and `Features:Authentication=false`.
- **Key Vault bootstrap** activates when `USE_KEYVAULT=true` environment variable and `KeyVault:VaultUri` are set.
- **Scalar UI** is enabled in Development; upgrade to `.NET 9` `MapOpenApi()` when ready.

## Scripts

- `scripts/dev-up.sh` – convenience launcher for API + web.
- `scripts/sanity-check.sh` – probes health endpoints, todos API, and docs UI.

## Front-end (React + Vite)

A React + Vite + TypeScript client lives in `apps/web` (mirroring the structure you liked in `React-ASP-NET Core 8 API`). Use `npm --prefix apps/web run dev` for the dev server or `npm --prefix apps/web run build` to produce static assets.

## Run the API from GHCR

### 1) Authenticate (if private image)

```bash
echo "$GHCR_PAT" | docker login ghcr.io -u YOUR_GH_USERNAME --password-stdin
```

### 2) Pull the image

```bash
docker pull ghcr.io/ira2222/app2:v0.1.0
docker pull ghcr.io/ira2222/app2@sha256:<DIGEST>
```

### 3) Run with Docker

```bash
docker run --rm -p 5080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  ghcr.io/ira2222/app2:v0.1.0
```

### 4) Or run with Docker Compose

```bash
# (A) Build locally
docker compose -f docker/compose.api.yml up --build -d

# (B) Pull from GHCR by tag
IMAGE=ghcr.io/ira2222/app2 TAG=v0.1.0 docker compose -f docker/compose.api.yml up -d

# (C) API + Redis (output caching enabled)
docker compose -f docker/compose.redis.yml up --build -d

# Stop
docker compose -f docker/compose.api.yml down -v
docker compose -f docker/compose.redis.yml down -v
```

> Compose variables can live in a `.env` file next to the compose file; see Docker’s interpolation precedence docs. ([Docker Documentation][2])
>
> `depends_on` ensures Redis starts before the API but does not wait for it to be healthy. The Redis service defines a healthcheck—wait for `docker compose ps` to show `(healthy)` (or use Compose v2 `condition: service_healthy`) before hitting the API.

## Next Steps

1. Author `openapi/app2.openapi.yaml` and wire automated client generation.
2. Add GitHub Actions to build, test, produce SBOM, and export Windows zip artifacts.
3. Introduce Redis-backed output caching toggle and distributed rate limit storage as needed.
