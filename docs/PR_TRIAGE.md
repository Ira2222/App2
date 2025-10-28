# PR Triage Plan

## Current Open PRs

Based on the branches we detected, here's the safe triage strategy:

---

## 1. Maintenance PRs (Pick One)

### ✅ **MERGE FIRST**: Your Comprehensive Sweep
**Branch:** `claude/session-011CUa1zjbVWEV7PqtmNM6Ts`

**What it contains:**
- 24 dependency updates (NuGet + npm)
- ValidateOnStart() configuration validation
- OpenSSF Scorecard workflow
- Security workflow hardening (fixed SARIF uploads)
- CSP + CORS tests
- Comprehensive SECURITY.md
- PR automation tools

**Action:** Wait for CI to pass, then merge
```bash
# Once CI is green
gh pr merge <PR_NUMBER> --squash --delete-branch
```

---

### 🔍 **REVIEW THEN CLOSE**: Other Maintenance PR
**Branch:** `codex/perform-maintenance-sweep-and-code-review`

**Action Required:**
1. Check what unique changes it has (if any)
2. If fully superseded by your PR → Close as superseded
3. If it has unique improvements → Cherry-pick, then close

**To compare:**
```bash
# View the other maintenance PR
gh pr view <OTHER_PR_NUMBER>

# Compare branches
git diff main...codex/perform-maintenance-sweep-and-code-review

# If superseded, close it
gh pr close <OTHER_PR_NUMBER> --comment "Closing as superseded by comprehensive maintenance sweep in PR #<YOUR_PR_NUMBER>"
```

---

## 2. Dependabot PRs - Triage by Type

### ❌ **CLOSE as Superseded** (Already in Your PR)

These updates are **already included** in your maintenance sweep:

**Trivy:**
- ❌ `dependabot/github_actions/aquasecurity/trivy-action-0.33.1`
  - **Why:** You already updated Trivy to 0.33.0 in security.yml
  - **Action:** Close with comment "Superseded by manual update in PR #<YOUR_PR>"

```bash
# Get the PR number for this branch
PR=$(gh pr list --head dependabot/github_actions/aquasecurity/trivy-action-0.33.1 --json number --jq '.[0].number')

# Close it
gh pr close $PR --comment "Superseded by comprehensive maintenance sweep that updated Trivy to 0.33.0"
```

**Check for overlapping package updates:**
```bash
# List all Dependabot nuget PRs
gh pr list --author app/dependabot --search "nuget" --json number,title,headRefName

# For each one, check if you already updated that package
# Close any that overlap with your 24 dependency updates
```

**Likely overlaps from your updates:**
- Azure.Identity (you updated to 1.13.1)
- FluentAssertions (you updated to 7.0.0)
- Any EF Core packages (you updated to 8.0.11)
- Newtonsoft.Json (you updated to 13.0.3)

---

### ⚠️ **REVIEW** (Major Version Updates)

**GitHub Actions v4 → v5:**
- ⚠️ `dependabot/github_actions/actions/checkout-5`
- ⚠️ `dependabot/github_actions/actions/upload-artifact-5`

**Why review:** Major version updates may have breaking changes

**Action:**
1. Check release notes for breaking changes
2. Test in your branch if needed
3. Either merge or configure Dependabot to group these

```bash
# View the PR to see what changed
gh pr view <PR_NUMBER>

# If safe, enable auto-merge
gh pr merge <PR_NUMBER> --squash --auto
```

**Alternative - Group future Actions updates:**

Add to `.github/dependabot.yml`:
```yaml
groups:
  github-actions:
    patterns:
      - "actions/*"
    update-types:
      - "minor"
      - "patch"
```

---

### ✅ **AUTO-MERGE** (Patch Updates)

**npm/yarn patches:**
- ✅ `dependabot/npm_and_yarn/apps/web/multi-*` (if patch-level)

**Action:** The Dependabot auto-merge workflow will handle these automatically!

**Manual enable if workflow isn't running yet:**
```bash
# List patch-level Dependabot PRs
gh pr list --author app/dependabot --json number,title,headRefName

# For each patch update, enable auto-merge
gh pr merge <PR_NUMBER> --squash --auto
```

---

## 3. Quick Command Reference

### Merge Your Maintenance PR (After CI Passes)

```bash
# Get your PR number
MY_PR=$(gh pr list --head claude/session-011CUa1zjbVWEV7PqtmNM6Ts --json number --jq '.[0].number')

# Check CI status
gh pr checks $MY_PR

# When green, merge
gh pr merge $MY_PR --squash --delete-branch
```

### Close Superseded Dependabot PRs

```bash
# Close Trivy update (already done in your PR)
gh pr close $(gh pr list --head dependabot/github_actions/aquasecurity/trivy-action-0.33.1 --json number --jq '.[0].number') \
  --comment "Superseded by comprehensive maintenance sweep"

# Check for overlapping NuGet updates
gh pr list --author app/dependabot --search "Azure.Identity OR FluentAssertions OR EntityFrameworkCore" \
  --json number,title --jq '.[] | "\(.number): \(.title)"'

# Close each overlapping one
gh pr close <NUMBER> --comment "Package already updated in maintenance sweep PR #$MY_PR"
```

### Auto-Merge Remaining Patch Updates

```bash
# Enable auto-merge for all open Dependabot PRs (safe ones will merge, others will wait for review)
gh pr list --author app/dependabot --json number --jq '.[].number' | while read n; do
  gh pr merge $n --squash --auto || echo "PR #$n needs manual review"
done
```

### Review Actions v5 Updates

```bash
# View the checkout v5 PR
gh pr view $(gh pr list --head dependabot/github_actions/actions/checkout-5 --json number --jq '.[0].number')

# View the upload-artifact v5 PR
gh pr view $(gh pr list --head dependabot/github_actions/actions/upload-artifact-5 --json number --jq '.[0].number')

# If safe, merge them
gh pr merge <NUMBER> --squash --auto
```

---

## 4. Recommended Order of Operations

**Step 1:** Wait for your maintenance PR CI to pass ✅

**Step 2:** Close superseded PRs:
- Trivy 0.33.1 (you already updated it)
- Any overlapping NuGet/npm updates

**Step 3:** Review and decide on Actions v5 updates:
- Read breaking changes
- Merge if safe, or defer

**Step 4:** Let Dependabot auto-merge workflow handle remaining patches ✅

**Step 5:** After your PR merges to main, create release tag v0.2.0 🎉

---

## 5. Long-Term: Configure Dependabot Grouping

Add to `.github/dependabot.yml` to reduce noise:

```yaml
version: 2
updates:
  - package-ecosystem: "nuget"
    directory: "/"
    schedule:
      interval: "weekly"
    groups:
      production-dependencies:
        patterns:
          - "*"
        update-types:
          - "patch"

  - package-ecosystem: "npm"
    directory: "/apps/web"
    schedule:
      interval: "weekly"
    groups:
      development-dependencies:
        patterns:
          - "*"
        update-types:
          - "patch"

  - package-ecosystem: "github-actions"
    directory: "/"
    schedule:
      interval: "weekly"
    groups:
      actions-updates:
        patterns:
          - "*"
        update-types:
          - "minor"
          - "patch"
```

**Result:** Dependabot will create one PR for all patch updates instead of individual PRs.

---

## Summary Checklist

- [ ] **Your maintenance PR**: Wait for CI, then merge
- [ ] **Other maintenance PR**: Review for unique changes, then close as superseded
- [ ] **Trivy Dependabot PR**: Close (already updated)
- [ ] **Overlapping package PRs**: Close (already updated)
- [ ] **Actions v5 PRs**: Review breaking changes, then merge or defer
- [ ] **Remaining patch PRs**: Auto-merge via workflow or manually enable
- [ ] **After merge**: Create release tag v0.2.0
- [ ] **Future**: Configure Dependabot grouping to reduce PR count

---

## Need Help?

**Check PR status:**
```bash
gh pr list --state open --json number,title,author,headRefName,mergeStateStatus
```

**View specific PR:**
```bash
gh pr view <NUMBER>
```

**Compare branches:**
```bash
git fetch origin
git log --oneline origin/main..origin/<branch-name>
```
