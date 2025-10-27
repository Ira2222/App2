# CI/CD Release Automation Scripts

Three practical bash workflows for different release scenarios using GitHub CLI for CI monitoring and attestation verification.

## Prerequisites

1. **GitHub CLI authenticated**
   ```bash
   gh auth status
   # If not authenticated:
   gh auth login
   ```

2. **Git configured** with remote origin pointing to GitHub repo

3. **Docker** (optional, only for GHCR image verification)

## Workflows

### 1. Full Release Cycle (`workflow-1-full-release.sh`)

**When to use:** Initial release or major version bump

**What it does:**
- Runs full test suite in Testing environment
- Pushes to main branch
- Waits for CI to complete
- Creates annotated tag and triggers release workflow
- Verifies release attestation and GHCR image provenance

**Usage:**
```bash
# Edit the config variables at the top of the script
OWNER="your-gh-owner"    # e.g., Ira2222
REPO="your-repo"         # e.g., App2
TAG="v0.1.0"

# Run the script
./scripts/workflow-1-full-release.sh
```

### 2. Quick Tag & Verify (`workflow-2-quick-tag.sh`)

**When to use:** Tests already passed, CI already green, just need to cut a tag

**What it does:**
- Creates annotated tag at HEAD
- Pushes tag (triggers release workflow)
- Verifies release attestation
- (Optional) Verifies specific asset file or GHCR image

**Usage:**
```bash
# Edit the config variables
OWNER="your-gh-owner"
REPO="your-repo"
TAG="vX.Y.Z"

# Run the script
./scripts/workflow-2-quick-tag.sh
```

### 3. Hotfix Tag (`workflow-3-hotfix-tag.sh`)

**When to use:** Need to tag an earlier commit for hotfix

**What it does:**
- Creates annotated tag at specified commit SHA
- Pushes tag (triggers release workflow)
- Verifies release attestation
- (Optional) Verifies GHCR image provenance

**Usage:**
```bash
# Edit the config variables
OWNER="your-gh-owner"
REPO="your-repo"
TAG="vX.Y.Z-hotfix"
COMMIT_SHA="abcdef1234567890"  # specific commit to tag

# Run the script
./scripts/workflow-3-hotfix-tag.sh
```

## Key Commands Explained

### `gh run watch --exit-status`
- Monitors the latest workflow run for current branch
- Blocks until completion
- Exits with non-zero code if workflow fails
- Use after push to gate tagging on CI success

### `gh release verify`
- Checks cryptographic signatures on release artifacts
- Validates SLSA provenance for immutable releases
- Ensures artifacts haven't been tampered with

### `gh release verify-asset`
- Verifies a local file against published release
- Useful for confirming local build matches published artifact

### `gh attestation verify oci://...`
- Validates container image provenance from GHCR
- Checks SLSA attestations for OCI images
- Ensures supply chain security for containers

## Common Issues & Solutions

### Issue: `gh run watch` picks wrong workflow
**Solution:** Filter by workflow name:
```bash
gh run list -R "${OWNER}/${REPO}" -w "release-provenance.yml" \
  --json databaseId -q '.[0].databaseId' | \
  xargs -I{} gh run watch --exit-status -R "${OWNER}/${REPO}" {}
```

### Issue: GHCR image is private
**Solution:** Login before verification:
```bash
echo $GHCR_PAT | docker login ghcr.io -u USERNAME --password-stdin
```

### Issue: Test results not visible
**Solution:** Check `TestResults/test-results.trx` file after test run

## Expected Outcomes

### Success (Workflow 1)
```
✅ Tests passed
✅ Pushed to main
✅ CI workflow completed successfully
✅ Tag v0.1.0 created and pushed
✅ Release verified
✅ Attestation verified for oci://ghcr.io/OWNER/REPO:v0.1.0
```

### Success (Workflow 2 & 3)
```
✅ Tag created and pushed
✅ Release verified
✅ (Optional) Attestation verified
```

### Failure Scenarios
- **Tests fail**: Script stops before push
- **CI fails**: `gh run watch` exits with error, no tag created
- **Attestation fails**: Indicates supply chain security issue

## Post-Execution Verification

1. Check GitHub releases: `https://github.com/OWNER/REPO/releases`
2. View workflow runs: `https://github.com/OWNER/REPO/actions`
3. Inspect GHCR package: `https://github.com/OWNER/REPO/pkgs/container/REPO`
4. Review test results: `TestResults/test-results.trx`

## Notes

- All three workflows are idempotent for tags (won't recreate existing tags)
- Workflow 1 includes full test suite with TRX logging
- Testing environment mirrors `appsettings.Testing.json` config
- Attestation verification provides SLSA supply chain guarantees
- Commands use standard Git and GitHub CLI patterns

## Copy-Paste Commands

If you prefer to run commands directly instead of using the scripts:

### Workflow 1 (Full Cycle)
```bash
# === CONFIG (edit the 3 lines below) ===
OWNER="your-gh-owner"
REPO="your-repo"
TAG="v0.1.0"

# === ENV for tests ===
export ASPNETCORE_ENVIRONMENT=Testing
export Features__KeyVault=false
export Features__Authentication=false
export Features__RedisOutputCache=false

# === 1) Test ===
dotnet restore
dotnet build -c Release
dotnet test -c Release --no-build --logger "trx;LogFileName=test-results.trx"

# === 2) Push ===
git add -A
git commit -m "chore: prepare ${TAG} (green tests)"
git push origin main

# === 3) Wait for CI ===
gh run watch --exit-status -R "${OWNER}/${REPO}"

# === 4) Tag & trigger release ===
git tag -a "${TAG}" -m "${TAG}"
git push origin "${TAG}"

# === 5) Verify attestations ===
gh release verify "${TAG}" -R "${OWNER}/${REPO}"
gh attestation verify "oci://ghcr.io/${OWNER}/${REPO}:${TAG}" --repo "${OWNER}/${REPO}"
```

### Workflow 2 (Quick Tag)
```bash
OWNER="your-gh-owner"; REPO="your-repo"; TAG="vX.Y.Z"

git tag -a "${TAG}" -m "${TAG}"
git push origin "${TAG}"

# Release attestation
gh release verify "${TAG}" -R "${OWNER}/${REPO}"

# (optional) verify specific asset
# gh release verify-asset "${TAG}" ./dist/your-asset.zip -R "${OWNER}/${REPO}"

# (optional) verify GHCR image
# gh attestation verify "oci://ghcr.io/${OWNER}/${REPO}:${TAG}" --repo "${OWNER}/${REPO}"
```

### Workflow 3 (Hotfix)
```bash
OWNER="your-gh-owner"; REPO="your-repo"
TAG="vX.Y.Z-hotfix"; COMMIT_SHA="abcdef1234567890"

git tag -a "${TAG}" "${COMMIT_SHA}" -m "${TAG}"
git push origin "${TAG}"

# Verify
gh release verify "${TAG}" -R "${OWNER}/${REPO}"
# gh attestation verify "oci://ghcr.io/${OWNER}/${REPO}:${TAG}" --repo "${OWNER}/${REPO}"
```
