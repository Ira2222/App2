# Template Changelog

Tracks notable updates that impact consumers of the App2 template. Use this log alongside `template-v*` tags so derivative apps can decide when to pull improvements forward.

## 2025-10-29

- Documented template lineage expectations (`README.md`, `TEMPLATE_ORIGIN.md` guidance)
- Added free-tier guardrails (pre-push hook, Gitleaks, Semgrep) and automated hook enablement in `scripts/new-project.*`
- Reaffirmed template-first workflow: App2 stays pristine; applications (e.g., LocationForm) live in their own repositories
