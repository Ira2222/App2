# Security Overview

This document outlines the security features, configurations, and best practices implemented in App2.

## Table of Contents

- [Security Features](#security-features)
- [CI/CD Security](#cicd-security)
- [Authentication & Authorization](#authentication--authorization)
- [Security Headers](#security-headers)
- [CORS Configuration](#cors-configuration)
- [Rate Limiting](#rate-limiting)
- [Dependency Management](#dependency-management)
- [Container Security](#container-security)
- [Secrets Management](#secrets-management)
- [Security Testing](#security-testing)

## Security Features

### Enabled by Default in Production

The following security features are enabled in production (`appsettings.Production.json`):

- ✅ **Authentication** - Microsoft Identity Web (JWT bearer tokens)
- ✅ **CORS** - Explicit origin allow-list (no wildcards)
- ✅ **Rate Limiting** - Fixed-window limiter with configurable limits
- ✅ **Output Caching** - With tag-based invalidation
- ✅ **Security Headers** - NetEscapades middleware with CSP
- ✅ **OpenTelemetry** - Observability and tracing

### Feature Toggles

All features can be toggled via `appsettings.json` → `Features` section:

```json
{
  "Features": {
    "Authentication": true,
    "CORS": true,
    "RateLimiting": true,
    "OutputCaching": true,
    "SecurityHeaders": true,
    "OpenTelemetry": true
  }
}
```

## CI/CD Security

### Static Code Analysis

- **CodeQL** - Runs on push/PR/schedule with security-extended queries
  - Languages: C#, JavaScript/TypeScript, GitHub Actions
  - Configuration: `.github/workflows/security.yml`

### Dependency Scanning

- **Trivy** - Scans repository and container images
  - Uploads SARIF results to GitHub Code Scanning
  - Configured for CRITICAL, HIGH, and MEDIUM severities
  - Configuration: `.github/workflows/security.yml`

### Supply Chain Security

- **SBOM Generation** - Anchore Syft generates CycloneDX SBOMs
  - Generated for repository and container images
  - Artifacts published with releases
  - Configuration: `.github/workflows/ci.yml`

- **SLSA Provenance** - Build attestation for releases and containers
  - Uses GitHub's `attest-build-provenance` action
  - Verifiable via `gh attestation verify`
  - Configuration: `.github/workflows/release-provenance.yml`, `.github/workflows/container.yml`

### Dependabot

Automated dependency updates configured for:
- NuGet packages (weekly)
- npm packages (weekly)
- GitHub Actions (weekly)

Configuration: `.github/dependabot.yml`

### Least-Privilege Permissions

All workflows use minimal required permissions:
- `contents: read` - Default for most jobs
- `security-events: write` - Only for SARIF upload
- `packages: write` - Only for container publishing
- `id-token: write` - Only for attestations

## Authentication & Authorization

### Azure AD Integration

Production environments require Azure AD configuration:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "YOUR-TENANT-ID",
    "ClientId": "YOUR-CLIENT-ID",
    "Audience": "api://YOUR-API-APPID"
  }
}
```

### Authorization Policies

Two scope-based policies are configured:

- `Todo.Read` - Required for GET operations
- `Todo.Write` - Required for POST, PUT, DELETE operations

Configuration: `Program.cs:61-65`

### Development Fallback

A development-only authentication handler (`DevHeader`) is available when:
- `ASPNETCORE_ENVIRONMENT=Development`
- `Features:Authentication=false`
- `Security:DevHeader:Enabled=true`

**⚠️ Never enable in production!**

## Security Headers

### NetEscapades Security Headers

Applied via middleware with the following configuration:

```csharp
policies.AddDefaultSecurityHeaders();
policies.RemoveCustomHeader("X-Frame-Options"); // Replaced by CSP
```

Configuration: `Extensions/SecurityHeadersExtensions.cs`

### Content Security Policy (CSP)

**Enabled by default in production** with the following directives:

- `default-src 'self'` - Only allow same-origin resources
- `script-src 'self' 'nonce-*'` - Scripts from same origin with nonce
- `style-src 'self' 'unsafe-inline'` - Styles from same origin + inline
- `img-src 'self' data:` - Images from same origin + data URIs
- `connect-src 'self'` - API calls to same origin only
- **`frame-ancestors 'none'`** - **Prevents clickjacking** (replaces X-Frame-Options)

Configuration: `Extensions/SecurityHeadersExtensions.cs:18-30`

### Why CSP frame-ancestors?

Per OWASP ASVS 4.0, CSP `frame-ancestors` is the modern replacement for the legacy `X-Frame-Options` header:

- ✅ More flexible and powerful
- ✅ Better browser support
- ✅ Part of CSP standard
- ❌ X-Frame-Options is deprecated

Reference: `Extensions/SecurityHeadersExtensions.cs:14-15`

## CORS Configuration

### Explicit Allow-List

CORS uses an explicit origin allow-list with **no wildcards**:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://your-frontend-domain.com"
    ]
  }
}
```

### Wildcard Protection

The application **rejects wildcard origins** when credentials are enabled:

```csharp
if (origins.Any(origin => origin == "*"))
{
    throw new InvalidOperationException(
        "Cors:AllowedOrigins must be explicit when credentials are enabled.");
}
```

Configuration: `Extensions/CorsExtensions.cs:19-23`

### CORS with Credentials

Credentials are enabled by default via `.AllowCredentials()`:

- Allows cookies and authorization headers
- Requires explicit origin list (no wildcards)
- Supports HTTP methods: GET, POST, PUT, DELETE, PATCH
- Allowed headers: Content-Type, Authorization

Configuration: `Extensions/CorsExtensions.cs:28-32`

## Rate Limiting

### Fixed-Window Limiter

Default configuration:

```json
{
  "RateLimiting": {
    "Fixed": {
      "PermitLimit": 100,
      "WindowSeconds": 60,
      "QueueLimit": 5
    }
  }
}
```

- 100 requests per 60-second window
- 5 requests can queue when limit reached
- Returns `429 Too Many Requests` with `Retry-After` header

Configuration: `Extensions/RateLimitingExtensions.cs`

## Dependency Management

### Package Update Policy

- ✅ **Patch updates** - Applied automatically
- ✅ **Minor updates** - Reviewed and applied when safe
- ⚠️ **Major updates** - Deferred unless security-critical

### Recent Updates (2025-10-28)

#### API Dependencies

- `Azure.Identity` 1.11.4 → 1.13.1
- `Microsoft.EntityFrameworkCore.*` 8.0.6 → 8.0.11
- `Microsoft.AspNetCore.OutputCaching.StackExchangeRedis` 8.0.0 → 8.0.11
- `Microsoft.Identity.Web` 3.8.2 → 3.8.3
- `NetEscapades.AspNetCore.SecurityHeaders` 0.21.0 → 0.24.2
- `Serilog.AspNetCore` 8.0.1 → 8.0.3
- `Serilog.Sinks.Console` 5.0.1 → 6.0.0
- `Swashbuckle.AspNetCore` 6.6.2 → 7.2.0
- `FluentValidation.DependencyInjectionExtensions` 11.9.0 → 11.11.0
- `OpenTelemetry.*` 1.9.0 → 1.10.0

#### Test Dependencies

- `Microsoft.NET.Test.Sdk` 17.11.1 → 17.12.0
- `FluentAssertions` 6.12.0 → 7.0.0
- `Microsoft.EntityFrameworkCore.InMemory` 8.0.6 → 8.0.11
- `Microsoft.AspNetCore.Mvc.Testing` 8.0.8 → 8.0.11
- `Microsoft.Data.Sqlite` 8.0.4 → 8.0.11
- `System.Collections.Immutable` 8.0.0 → 9.0.0
- `Newtonsoft.Json` 13.0.1 → 13.0.3

## Container Security

### Multi-Stage Build

Dockerfile uses multi-stage build to minimize attack surface:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# ... build stage ...

FROM mcr.microsoft.com/dotnet/aspnet:8.0
# ... runtime stage (minimal)
```

Configuration: `src/App2.Api/Dockerfile`

### Non-Root User

Container runs as non-root user (default in aspnet base image).

### Health Checks

Container includes health check configuration:

```dockerfile
HEALTHCHECK --interval=10s --timeout=3s --start-period=20s --retries=5 \
  CMD curl -fsS http://localhost:8080/healthz/ready || exit 1
```

### Image Scanning

All built images are scanned with Trivy for vulnerabilities:
- Scan results uploaded to GitHub Code Scanning
- Configured for CRITICAL and HIGH severities
- Fails build on critical vulnerabilities

Configuration: `.github/workflows/security.yml:59-81`

### Multi-Architecture Support

Containers built for:
- `linux/amd64`
- `linux/arm64`

Configuration: `.github/workflows/container.yml:56`

## Secrets Management

### Azure Key Vault Integration

Production environments can use Azure Key Vault:

```bash
export USE_KEYVAULT=true
```

Required configuration:

```json
{
  "KeyVault": {
    "VaultUri": "https://your-vault.vault.azure.net/"
  }
}
```

### Environment-Specific Secrets

- Development: Local configuration files (never committed)
- Production: Azure Key Vault or environment variables
- CI/CD: GitHub Secrets

### Fail-Fast Validation

Application validates required configuration at startup in non-development environments:

- Azure AD Tenant ID and Client ID
- CORS allowed origins
- Redis connection string (if Redis caching enabled)

Configuration: `Extensions/ConfigurationValidationExtensions.cs`

## Security Testing

### Integration Tests

Comprehensive security tests verify:

- ✅ Rate limiting enforcement (`TodosEndpoint_IsRateLimited`)
- ✅ Security headers presence (`SecurityHeaders_ArePresent`)
- ✅ Validation error responses (`CreateTodo_WithInvalidPayload_ReturnsBadRequest`)
- ✅ Not-found handling (`GetTodoById_ReturnsNotFound_WhenMissing`)
- ✅ CRUD authorization (when auth enabled)
- ✅ CORS preflight handling (`CorsPreflightRequest_ReturnsCorrectHeaders`)

Configuration: `tests/App2.Tests.Integration/ModularFeaturesTests.cs`

### Unit Tests

Comprehensive unit tests for:

- Command handlers (Create, Update, Delete)
- Query handlers (GetAll, GetById)
- Validators (FluentValidation rules)
- Repository operations

Configuration: `tests/App2.Tests.Unit/`

### Code Coverage

Coverage collected via:
- Coverlet (XPlat Code Coverage)
- Reports uploaded as CI artifacts

Configuration: `.github/workflows/ci.yml:34`

## Security Checklist

### Before Deploying to Production

- [ ] Set `Features:Authentication=true`
- [ ] Configure Azure AD `TenantId`, `ClientId`, `Audience`
- [ ] Set explicit `Cors:AllowedOrigins` (no wildcards)
- [ ] Enable `Features:SecurityHeaders=true`
- [ ] Enable `Features:RateLimiting=true`
- [ ] Configure Redis if using `RedisOutputCache=true`
- [ ] Set `USE_KEYVAULT=true` and configure Key Vault URI
- [ ] Review and enable OpenTelemetry endpoint
- [ ] Verify all tests pass in production-like environment
- [ ] Run container vulnerability scans
- [ ] Review SBOM for unexpected dependencies

### Ongoing Maintenance

- [ ] Review Dependabot PRs weekly
- [ ] Monitor CodeQL alerts
- [ ] Review Trivy scan results
- [ ] Update base images monthly
- [ ] Rotate secrets quarterly
- [ ] Review access logs for anomalies
- [ ] Test disaster recovery procedures

## Security Contacts

For security issues, please contact:

- Security Team: [Insert contact]
- On-call: [Insert on-call contact]

**Do not open public issues for security vulnerabilities.**

## References

- [OWASP ASVS 4.0](https://owasp.org/www-project-application-security-verification-standard/)
- [OWASP CSP Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Content_Security_Policy_Cheat_Sheet.html)
- [OpenSSF Scorecard](https://scorecard.dev/)
- [GitHub Security Best Practices](https://docs.github.com/en/code-security)
- [Microsoft Identity Platform](https://learn.microsoft.com/en-us/entra/identity-platform/)
- [Trivy Documentation](https://trivy.dev/)
- [SLSA Framework](https://slsa.dev/)
