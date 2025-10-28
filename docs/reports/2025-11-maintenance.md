# November 2025 Maintenance Sweep

## Dependency refresh
- **Azure.Identity** → 1.17.0 ([release notes](https://github.com/Azure/azure-sdk-for-net/releases/tag/Azure.Identity_1.17.0))
- **Microsoft.AspNetCore.OutputCaching.StackExchangeRedis** → 8.0.16 ([release notes](https://github.com/dotnet/aspnetcore/releases/tag/v8.0.16))
- **Entity Framework Core toolchain/tests** → 8.0.11 (`Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.InMemory`, `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.Data.Sqlite`) ([release notes](https://github.com/dotnet/efcore/releases/tag/v8.0.11))

## Security & platform hygiene
- Ensure NetEscapades security headers execute first in the request pipeline for consistent coverage.
- Tighten the CORS allow-list policy to always emit `Vary: Origin` and reject wildcard origins when credentials are allowed.
- Add `DatabaseOptions` with options validation so production/startup fails fast if the selected provider lacks a connection string.

## Follow-up considerations
- Monitor Azure SDK release cadence for identity/authentication improvements (next review February 2026).
- Consider onboarding `dotnet outdated` tool manifest to capture advisory metadata in future sweeps.
