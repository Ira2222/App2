# DEV-AI Brief

**Architecture**
- Preserve Clean Architecture boundaries (Domain → Application → Infrastructure → API).
- Use MediatR for commands/queries; validate requests with FluentValidation before hitting handlers.
- Keep Infrastructure concerns (EF, external services) out of Application/Domain.

**Security**
- Enforce guardrails already included (ProblemDetails, rate limiting, security headers).
- When enabling Authentication, prefer `RequireScope` / `RequireAuthorization` helpers and document policies in README.

**Testing**
- Unit tests in `tests/App2.Tests.Unit` (add more projects as needed).
- Integration tests in `tests/App2.Tests.Integration`; adapt fixtures to new infrastructure (Redis, Postgres, etc.).

**Frontend**
- API client is generated from OpenAPI spec; regenerate after contract updates (`npm --prefix apps/web run generate:client`).
- Keep typings in sync with backend DTOs.

**Definition of done**
1. `dotnet build` + `dotnet test` succeed.
2. `npm --prefix apps/web run build` succeeds.
3. Docs updated (`README`, `config/schema.yaml`) with new knobs.
4. CI lint/security workflows pass.
