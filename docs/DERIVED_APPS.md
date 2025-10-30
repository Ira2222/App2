# Derived Applications

Track production apps built from App2 so template consumers know which repositories demonstrate specific features. Keep entries lightweight and reference the tag/commit recorded in each child repo’s `TEMPLATE_ORIGIN.md`.

| App | Baseline | Notes |
| --- | --- | --- |
| [LocationForm](https://github.com/Ira2222/LocationForm) | `template-v1.0.0` | Reference implementation for Entra ID auth (API + MSAL SPA), Testcontainers integration tests, Windows bundle export, config schema docs, and `/agents` starter pack. |

When you cut a new template tag:

1. Update `TEMPLATE_CHANGELOG.md` with the highlights.
2. Add or adjust entries here so downstream teams can discover real-world usage.
3. Encourage derived repos to refresh their `TEMPLATE_ORIGIN.md` to the new tag if they adopt the changes.
