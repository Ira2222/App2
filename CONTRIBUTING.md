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

## Repository Setup (for New Projects from Template)

When you create a new project from this template, configure these settings:

### 1. Branch Protection

Protect the `main` branch to enforce quality standards:

1. Go to **Settings → Branches → Add rule**
2. Branch name pattern: `main`
3. Enable:
   - ✅ **Require a pull request before merging**
     - Required approvals: 1
   - ✅ **Require status checks to pass before merging**
     - Required checks: `ci`, `container`, `security`
   - ✅ **Require conversation resolution before merging**
   - ✅ **Do not allow bypassing the above settings**
4. Disable:
   - ❌ Allow force pushes
   - ❌ Allow deletions

[GitHub Docs: Branch Protection](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/managing-a-branch-protection-rule)

### 2. GitHub Environments (Optional)

For deployment workflows, create environments with protection rules:

1. Go to **Settings → Environments**
2. Create environments: `dev`, `staging`, `prod`
3. For `staging` and `prod`:
   - Add required reviewers
   - Set deployment branch: `main` only
   - Add environment secrets (API keys, connection strings, etc.)

[GitHub Docs: Environments](https://docs.github.com/en/actions/deployment/targeting-different-environments/using-environments-for-deployment)

### 3. Code Security

Enable security features:

1. **Settings → Security → Code scanning**
   - CodeQL analysis is already configured via workflow
   - Review alerts regularly

2. **Settings → Security → Secret scanning**
   - Enable for public and private repos

3. **Settings → Security → Dependabot**
   - Alerts: Already enabled
   - Security updates: Enable if desired
   - Version updates: Configured via `.github/dependabot.yml`

### 4. Update CODEOWNERS

Edit `.github/CODEOWNERS` to reflect your team:
```
# Replace @Ira2222 with your team/username
* @your-team
/src/App2.Api/ @backend-team
/apps/web/ @frontend-team
```

### 5. Repository Settings

Recommended settings:
- **Settings → General**
  - ✅ Allow squash merging (default)
  - ✅ Automatically delete head branches
  - ✅ Always suggest updating pull request branches
- **Settings → Actions**
  - Workflow permissions: Read and write (for SBOM/attestation)
