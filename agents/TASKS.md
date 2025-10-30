# Agent Task Playbook

Common commands for any repo generated from App2:

- Create feature branch: `git checkout -b feature/<short-name>`
- Run solution tests: `dotnet test`
- Run integration tests: `dotnet test tests/App2.Tests.Integration/App2.Tests.Integration.csproj`
- Build web assets: `npm --prefix apps/web run build`
- Review configuration knobs: see `config/schema.yaml`
- Before shipping: ensure CI is green and security scans (Semgrep, Gitleaks, Trivy) are addressed
