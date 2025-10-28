# Maintenance Sweep — November 2024

## Review highlights
- Output cache feature flag hardened so disabling the feature no longer breaks DI or endpoint caching configuration.
- Security headers middleware now runs earlier in the pipeline to guarantee headers on redirect responses.

## Outstanding issues to track
1. Minimal API endpoints always enabled output caching, which crashed when the feature flag was off (fixed in this sweep).
2. Security headers were applied after HTTPS redirection, so redirect responses missed CSP/HSTS hardening (fixed in this sweep).
3. Align `Microsoft.EntityFrameworkCore.Design` and `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` with the 8.0.11 patch that Infrastructure already consumes.
4. `Microsoft.AspNetCore.OutputCaching.StackExchangeRedis` is still on the 8.0.0 RTM build; bump to 8.0.11 for the latest fixes.
5. Test projects lag on EF Core packages (`InMemory`, `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.Data.Sqlite`)—all have 8.0.11 patches available.
6. Azure SDK dependencies (`Azure.Identity`, `Azure.Core`) should be checked for the latest servicing release; Dependabot will surface once policy allows.

## Dependency observations
- `.NET` — Recommend upgrading the EF Core and output caching packages noted above to their 8.0.11 service releases.
- `npm` — `npm outdated` shows React 18.x stack is current within its major; no action required now.
- Dependabot is enabled for NuGet, npm, and GitHub Actions and will keep surfacing new advisories.

## Next steps
- Schedule a follow-up PR to apply the recommended package bumps and regenerate lock files once a .NET SDK is available in CI.
- Review outstanding CodeQL and Trivy SARIF reports to ensure no new actionable alerts have appeared since the last scan.
- Consider adding smoke tests that exercise the API with OutputCaching disabled to guard against regressions like the one fixed here.
