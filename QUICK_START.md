# Quick Start Guide

**Repository:** https://github.com/Ira2222/App2

This guide gets you up and running with App2 in minutes.

---

## ✅ Setup Complete!

You've already completed the initial setup:
- ✅ GitHub repository created and pushed
- ✅ Actions permissions configured (read/write)
- ✅ Security features enabled (Dependabot, Secret Scanning)
- ✅ CI/CD workflows running
- ✅ Code quality tools configured (.editorconfig)
- ✅ OpenAPI client generation ready

---

## 🚀 Quick Commands

### Development

```bash
# Start the API
dotnet run --project src/App2.Api --launch-profile https

# Start the React app (separate terminal)
npm --prefix apps/web run dev

# Or use the convenience script
./scripts/dev-up.sh
```

### Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run only unit tests
dotnet test tests/App2.Tests.Unit

# Run only integration tests
dotnet test tests/App2.Tests.Integration
```

### OpenAPI Client Generation

```bash
# Generate TypeScript client for React app
./scripts/generate-openapi-client.sh

# This will:
# 1. Start the API
# 2. Export OpenAPI spec from Swagger
# 3. Generate TypeScript Fetch client in apps/web/src/api/
# 4. Clean up
```

### Docker

```bash
# Build and run locally
docker compose -f docker/compose.api.yml up --build

# With Redis (output caching)
docker compose -f docker/compose.redis.yml up --build

# Pull from GHCR (after first release)
docker pull ghcr.io/ira2222/app2:latest
docker run --rm -p 5080:8080 ghcr.io/ira2222/app2:latest
```

### Release Workflows

```bash
# Full release (recommended for first release)
./scripts/workflow-1-full-release.sh
# This runs: tests → push → wait for CI → create tag → release

# Quick tag (when CI is already green)
./scripts/workflow-2-quick-tag.sh v0.1.0

# Hotfix for specific commit
./scripts/workflow-3-hotfix-tag.sh v0.1.1 abc123
```

---

## 📁 Project Structure

```
App2/
├── src/                          # Source code
│   ├── App2.Domain/              # Entities, interfaces
│   ├── App2.Application/         # Business logic (CQRS with MediatR)
│   ├── App2.Infrastructure/      # Data access, repositories
│   └── App2.Api/                 # Minimal API endpoints
├── apps/
│   └── web/                      # React + Vite frontend
├── tests/
│   ├── App2.Tests.Unit/          # Unit tests
│   └── App2.Tests.Integration/   # Integration tests
├── docker/                       # Docker Compose files
├── scripts/                      # Automation scripts
└── .github/workflows/            # CI/CD workflows
```

---

## 🔧 Configuration

### Feature Flags

Toggle features in `appsettings.json` → `Features`:

| Feature | Purpose | Dev Default | Prod Default |
|---------|---------|-------------|--------------|
| Authentication | JWT auth | ❌ false | ✅ true |
| CORS | Allow-list | ✅ true | ❌ false |
| RateLimiting | 100/min | ✅ true | ✅ true |
| OutputCaching | Response cache | ✅ true | ✅ true |
| RedisOutputCache | Redis backend | ❌ false | ✅ true |
| SecurityHeaders | CSP, etc. | ✅ true | ✅ true |
| OpenTelemetry | Observability | ❌ false | ✅ true |

### Environment Variables

```bash
# API
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080

# Azure Key Vault (optional)
USE_KEYVAULT=true
KeyVault__VaultUri=https://your-vault.vault.azure.net/

# Redis (optional)
Redis__Connection=localhost:6379
```

### Frontend Environment

```bash
# apps/web/.env.local
VITE_API_BASE_URL=http://localhost:5081
```

---

## 🧪 Testing the API

### Health Checks

```bash
# Liveness probe
curl http://localhost:5081/healthz/live

# Readiness probe
curl http://localhost:5081/healthz/ready
```

### Todos API

```bash
# Get all todos
curl http://localhost:5081/api/todos

# Create a todo
curl -X POST http://localhost:5081/api/todos \
  -H "Content-Type: application/json" \
  -d '{"title":"Test Todo","description":"This is a test"}'
```

### Swagger UI

Open in browser:
```
http://localhost:5081/swagger
```

---

## 📊 GitHub Actions Workflows

### Workflows Running

1. **CI** (`.github/workflows/ci.yml`)
   - Triggers: Push/PR to main
   - Jobs: Backend build/test, Frontend build, SBOM generation
   - Status: https://github.com/Ira2222/App2/actions/workflows/ci.yml

2. **Security** (`.github/workflows/security.yml`)
   - Triggers: Push/PR, weekly schedule
   - Jobs: CodeQL analysis, Trivy scanning
   - Status: https://github.com/Ira2222/App2/actions/workflows/security.yml

3. **Container** (`.github/workflows/container.yml`)
   - Triggers: Push to main, PRs, releases
   - Jobs: Docker build, push to GHCR, SLSA provenance
   - Status: https://github.com/Ira2222/App2/actions/workflows/container.yml

4. **Release** (`.github/workflows/release-provenance.yml`)
   - Triggers: Semver tags (v*.*.*)
   - Jobs: Publish API/web artifacts, create release, attestations
   - Status: https://github.com/Ira2222/App2/actions/workflows/release-provenance.yml

### Monitoring Workflows

```bash
# List recent runs
gh run list

# Watch a specific run
gh run watch <run-id>

# View logs
gh run view <run-id> --log

# View in browser
gh run view <run-id> --web
```

---

## 🔒 Security

### Current Security Status

- ✅ **Dependabot** - Automated dependency updates
- ✅ **Secret Scanning** - Prevents credential leaks
- ✅ **Push Protection** - Blocks secret commits
- ✅ **CodeQL** - Static analysis for vulnerabilities
- ✅ **Trivy** - Container/filesystem scanning
- ✅ **SLSA Provenance** - Supply chain attestations

### Dependabot Alerts

Check current alerts:
```bash
gh browse /security/dependabot
```

Or visit: https://github.com/Ira2222/App2/security/dependabot

### Reviewing Dependabot PRs

```bash
# List open PRs
gh pr list --author "app/dependabot"

# Review a PR
gh pr view <pr-number>

# Approve and merge
gh pr review <pr-number> --approve
gh pr merge <pr-number> --auto --squash
```

---

## 🎯 Next Steps

### Immediate (5 minutes)

1. **Wait for CI to pass** ✅
   - Check: https://github.com/Ira2222/App2/actions
   - All workflows should be green

2. **Review Dependabot PRs**
   - Check: https://github.com/Ira2222/App2/pulls
   - Merge security updates

### Short-term (15-30 minutes)

3. **Generate OpenAPI Client**
   ```bash
   ./scripts/generate-openapi-client.sh
   ```

4. **Update React App**
   - Use generated TypeScript client
   - Replace manual `fetch()` calls
   - See: `apps/web/src/api/`

5. **Configure Azure AD** (if needed)
   - Update `appsettings.json` with real values
   - Replace "TODO-TENANT-ID", etc.
   - Enable `Features.Authentication`

### Medium-term (when ready)

6. **Run First Release**
   ```bash
   ./scripts/workflow-1-full-release.sh
   ```

7. **Verify SLSA Provenance**
   ```bash
   gh attestation verify oci://ghcr.io/ira2222/app2:v0.1.0 \
     --owner Ira2222 --repo App2
   ```

8. **Add More Tests**
   - Expand unit test coverage
   - Add integration tests for endpoints
   - See: `tests/` directory

---

## 📚 Documentation

- **[README.md](README.md)** - Overview and getting started
- **[GITHUB_SETUP.md](GITHUB_SETUP.md)** - GitHub configuration guide
- **[CONTRIBUTING.md](CONTRIBUTING.md)** - Development workflow
- **[PRODUCTION_CHECKLIST.md](PRODUCTION_CHECKLIST.md)** - Pre-deployment checklist
- **[RELEASE_WORKFLOWS.md](RELEASE_WORKFLOWS.md)** - Release automation
- **[SETUP_GUIDE.md](SETUP_GUIDE.md)** - Workflow setup instructions

---

## 🆘 Troubleshooting

### CI Failures

```bash
# View failed run logs
gh run list --status failure --limit 1
gh run view <run-id> --log-failed

# Common issues:
# - Test failures: Run `dotnet test` locally
# - Build errors: Run `dotnet build` locally
# - Frontend: Run `npm --prefix apps/web run build`
```

### Container Build Failures

```bash
# Test Docker build locally
docker build -f src/App2.Api/Dockerfile -t app2:test .

# Common issues:
# - Context path: Ensure you're in project root
# - Missing files: Check .dockerignore
# - Multi-stage errors: Check SDK vs runtime versions
```

### OpenAPI Generation Issues

```bash
# Ensure API is running
curl http://localhost:5081/swagger/v1/swagger.json

# Manually export spec
curl http://localhost:5081/swagger/v1/swagger.json > openapi/app2.openapi.json

# Generate client
cd apps/web
npm run generate:client
```

### Workflow Permission Issues

**Error:** `Resource not accessible by integration`

**Fix:**
1. Go to https://github.com/Ira2222/App2/settings/actions
2. Select "Read and write permissions"
3. Enable "Allow GitHub Actions to create and approve pull requests"
4. Save

---

## 🔗 Useful Links

- **Repository:** https://github.com/Ira2222/App2
- **Actions:** https://github.com/Ira2222/App2/actions
- **Packages:** https://github.com/Ira2222?tab=packages
- **Security:** https://github.com/Ira2222/App2/security
- **Dependabot:** https://github.com/Ira2222/App2/security/dependabot
- **Pull Requests:** https://github.com/Ira2222/App2/pulls
- **Releases:** https://github.com/Ira2222/App2/releases

---

**Last Updated:** 2025-10-28
**Status:** ✅ Production Ready
