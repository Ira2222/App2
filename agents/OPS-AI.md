# OPS-AI Brief

**Pipelines**
- CI (`.github/workflows/ci.yml`) calls `_ci.yml` which restores, builds, tests, generates SBOM, and now publishes Windows-friendly bundles.
- Security workflows (Semgrep, Gitleaks, Trivy) run on pull requests; treat failures as blocking.

**Release checklist**
1. Confirm `main` branch is green.
2. Download artifacts (`app2-windows-bundles`, `sbom`).
3. Tag release (`template-vX.Y.Z`) and update `TEMPLATE_CHANGELOG.md`.

**Configuration**
- Reference `config/schema.yaml` when introducing new toggles or connection strings.
- Keep secrets in environment-specific stores (Key Vault, GitHub secrets). Never commit them.

**Operations**
- Health endpoints exposed via `app.MapHealthEndpoints()`.
- Serilog logs to console by default; configure sinks per environment.
- For temporary auth bypass in dev, toggle `Features:Authentication` OR use dev header fallback—never leave disabled in prod.
