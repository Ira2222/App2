# Production Checklist

Use this list before promoting App2 to staging or production.

- [ ] **Authentication configured**
  - Azure AD App Registration created (Public or US Gov audience).
  - `AzureAd:{TenantId,ClientId,Audience}` populated with real values.
  - App roles and scopes documented.
- [ ] **CORS allow-list locked down**
  - `Cors:AllowedOrigins` enumerates explicit origins (no wildcards).
  - Non-production origins removed from Production config.
- [ ] **Security headers validated**
  - NetEscapades defaults active.
  - CSP override tuned for hosted static assets (nonces/hashes, no unsafe-inline/eval).
- [ ] **Rate limiting & caching**
  - Fixed policy values matched to load profile.
  - Output cache (optionally Redis) sized; invalidation tags tested.
- [ ] **Health & readiness**
  - `/healthz/live` wired to platform liveness probe.
  - `/healthz/ready` includes DB + external dependencies.
  - Alerts configured for unhealthy status.
- [ ] **Telemetry pipeline**
  - `OpenTelemetry:Endpoint` set, exporter reachable.
  - Logs ship centrally (Serilog sinks configured).
- [ ] **Secrets management**
  - `USE_KEYVAULT=true` in deployment environment.
  - Managed Identity or service principal assigned Key Vault access.
- [ ] **Data provider**
  - Production connection string stored in secrets vault.
  - Migration strategy documented (EF migrations, backups).
- [ ] **CI/CD safeguards**
  - GitHub Actions run build, tests, SBOM, vulnerability scanners (CodeQL, Trivy).
  - Release pipeline generates Windows zip artifact for smoke tests.
- [ ] **Documentation**
  - README updated with environment-specific notes.
  - Runbooks and on-call procedures stored in `.project/` or Knowledge tree.
