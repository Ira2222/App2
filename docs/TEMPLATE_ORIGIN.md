# Template Lineage Guidance

Every application created from App2 must include a `TEMPLATE_ORIGIN.md` file in the repository root. The file provides traceability back to the template version that seeded the app and captures any day-zero customisations.

Recommended structure:

```md
# Template Origin

- Template: https://github.com/Ira2222/App2
- Template release/tag: template-vX.Y.Z
- Commit SHA: <copy-from-App2>
- Generated on: YYYY-MM-DD
- Post-clone steps: rename-from-template, env setup, guardrails, etc.
- Initial customisations: Notes about deleted samples, added secrets, bootstrap data, etc.
```

Why it matters:

- **Traceability** – quickly identify which template changes apply to a project.
- **Upgrade path** – compare the recorded tag/SHA with `TEMPLATE_CHANGELOG.md` to decide when to pull improvements.
- **Operational clarity** – new contributors or agents can see how close the app is to the baseline template.

Pair this file with a short “Template Lineage” section in each derived app’s `README.md` so the relationship is visible without opening additional docs.
