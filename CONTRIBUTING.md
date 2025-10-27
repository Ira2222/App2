# Contributing Guidelines

We practice direct, candid reviews and expect every change to include tests (or a rationale when tests are not possible).

## Workflow

1. **Sync context**
   - Read `.project/CONTEXT.md` (create if missing) and current specs/Beads entries.
   - Note existing feature flags and configuration deltas.
2. **Branch naming**
   - Use `feature/<slug>` or `fix/<slug>`; keep slugs short.
3. **Make the change**
   - Keep API endpoints thin; push logic into Application layer.
   - Honor feature toggles—never remove a flag without updating docs.
4. **Tests**
   - `dotnet test` for unit/integration suites.
   - Add/extend integration tests when touching middleware, auth, or rate limiting.
5. **Docs + context**
   - Update README/PRODUCTION_CHECKLIST if behavior or setup changes.
   - Record a CHECKPOINT entry in `.project/CONTEXT.md` referencing impacted beads/tasks.
6. **Review checklist**
   - [ ] Tests added/updated
   - [ ] Configuration defaults safe
   - [ ] ProblemDetails shape preserved
   - [ ] Health endpoints unaffected (or tests updated)
   - [ ] No secrets/framework credentials committed

## Commit Guidelines

Use Conventional Commits:

- `feat(api): add todos cache invalidation`
- `fix(auth): align cloud authority`
- `chore(ci): publish sbom artifact`
- `docs: update production checklist`

Keep PRs concise (< 300 LOC diff when possible). If a change is necessarily large, outline a plan with checkpoints and land it in stages.
