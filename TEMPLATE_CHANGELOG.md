# Template Changelog

Tracks notable updates that impact consumers of the App2 template. Use this log alongside `template-v*` tags so derivative apps can decide when to pull improvements forward.

## template-v1.0.0 · 2025-10-29

- Documented template lineage expectations (`README.md`, `TEMPLATE_ORIGIN.md`) so every derived app records its baseline.
- Added free-tier guardrails: repo-level pre-push hook, Gitleaks & Semgrep workflows, and automated hook enablement in `scripts/new-project.*`.
- Reaffirmed template-first workflow: App2 stays pristine; applications (e.g., LocationForm) live in their own repositories.
- Added Windows bundle export job to `_ci.yml` for downstream convenience.
- Documented configuration surface in `config/schema.yaml` to reduce per-app drift.
- Introduced `/agents` starter pack (PM/DEV/OPS briefs + task playbook).
- README now links to LocationForm as a reference implementation and highlights the new artifacts.
