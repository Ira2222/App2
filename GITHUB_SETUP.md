# GitHub Repository Setup Guide

This guide covers the one-time manual configuration steps required to enable all GitHub Actions workflows in your App2 repository.

## Repository URL

**https://github.com/Ira2222/App2**

---

## 1. Configure Actions Permissions (CRITICAL)

### Why This Is Needed
The release and container workflows need to:
- Create GitHub releases
- Push Docker images to GitHub Container Registry (GHCR)
- Create SLSA provenance attestations
- Write artifacts and comments

### Steps

1. Navigate to your repository: https://github.com/Ira2222/App2
2. Click **Settings** (top navigation bar)
3. In the left sidebar, click **Actions** → **General**
4. Scroll down to **Workflow permissions**
5. Select **"Read and write permissions"**
6. ✅ Check **"Allow GitHub Actions to create and approve pull requests"**
7. Click **Save**

### Verification
After saving, you should see:
```
✓ Workflows have read and write permissions in the repository for all scopes
✓ Workflows can approve pull requests
```

**Reference:** [Microsoft Learn - Rate limiting](https://learn.microsoft.com/en-us/dotnet/core/extensions/http-ratelimiter)

---

## 2. Enable Security Features

### Code Scanning (CodeQL)

1. Go to **Settings** → **Code security and analysis**
2. Under **Code scanning**, click **Set up** → **Default**
3. This enables automatic CodeQL analysis for C# and TypeScript

**Note:** Your repository already has a custom CodeQL workflow at `.github/workflows/security.yml`, so this step is optional but recommended for GitHub's native integration.

### Dependabot Alerts

1. Go to **Settings** → **Code security and analysis**
2. Enable **Dependabot alerts**
3. Enable **Dependabot security updates**

**Note:** Your repository already has Dependabot configured via `.github/dependabot.yml`.

### Secret Scanning

1. Go to **Settings** → **Code security and analysis**
2. Enable **Secret scanning**
3. Enable **Push protection** (prevents accidental secret commits)

---

## 3. Configure GitHub Container Registry (GHCR)

### Make Package Public (Optional)

By default, packages pushed to GHCR are private. To make your Docker image public:

1. Navigate to https://github.com/Ira2222?tab=packages
2. Click on the **app2** package (after your first container workflow runs)
3. Click **Package settings** (right sidebar)
4. Scroll down to **Danger Zone**
5. Click **Change visibility** → **Public**

### Authentication for Pulling Private Images

If you keep the image private, you'll need to authenticate:

```bash
# Create a Personal Access Token (PAT) with read:packages scope
# Go to: https://github.com/settings/tokens/new

# Login to GHCR
echo "$GITHUB_PAT" | docker login ghcr.io -u Ira2222 --password-stdin

# Pull the image
docker pull ghcr.io/ira2222/app2:v0.1.0
```

---

## 4. Configure Branch Protection (Recommended)

Protect your `main` branch to ensure quality:

1. Go to **Settings** → **Branches**
2. Click **Add branch protection rule**
3. Branch name pattern: `main`
4. Enable:
   - ✅ **Require a pull request before merging**
     - Require approvals: 1
   - ✅ **Require status checks to pass before merging**
     - Search and add: `.NET build & test`, `Web build (Vite)`, `CodeQL`
   - ✅ **Require conversation resolution before merging**
   - ✅ **Do not allow bypassing the above settings**
5. Click **Create**

**Note:** If you're working solo, you can skip this or allow yourself to bypass via "Allow specified actors to bypass required pull requests".

---

## 5. Verify Workflows Are Running

### Check Workflow Status

1. Navigate to https://github.com/Ira2222/App2/actions
2. You should see workflows running for your latest push
3. Verify all workflows complete successfully:
   - ✅ **CI** - .NET build, test, frontend build, SBOM generation
   - ✅ **Security** - CodeQL analysis, Trivy scanning
   - ✅ **Container** - Docker build (triggers on main push)

### Troubleshooting Failed Workflows

#### CI Workflow Failures
```bash
# Common issues:
# - Missing dependencies: Run `dotnet restore` locally
# - Test failures: Run `dotnet test` locally
# - Frontend build: Run `npm --prefix apps/web run build`

# Check logs:
gh run view <run-id> --log
```

#### Container Workflow Failures
```bash
# Common issues:
# - Permissions: Ensure "Read and write permissions" is enabled
# - Docker build context: Check Dockerfile path

# Verify locally:
docker build -f src/App2.Api/Dockerfile -t app2:test .
```

#### Security Workflow Failures
```bash
# CodeQL may take longer on first run (10-15 minutes)
# Trivy may fail if:
# - Network issues downloading vulnerability database
# - Critical vulnerabilities found (check and update dependencies)
```

---

## 6. Configure Repository Secrets (Optional)

For advanced scenarios like Azure Key Vault, configure repository secrets:

1. Go to **Settings** → **Secrets and variables** → **Actions**
2. Click **New repository secret**
3. Add secrets as needed:
   - `AZURE_TENANT_ID` - Azure AD tenant ID
   - `AZURE_CLIENT_ID` - Service principal client ID
   - `AZURE_CLIENT_SECRET` - Service principal secret
   - `KEYVAULT_URI` - Azure Key Vault URI

### Example: Using Secrets in Workflows

```yaml
# .github/workflows/ci.yml
env:
  AZURE_TENANT_ID: ${{ secrets.AZURE_TENANT_ID }}
  AZURE_CLIENT_ID: ${{ secrets.AZURE_CLIENT_ID }}
```

---

## 7. Configure Environments (Optional)

Create deployment environments for production releases:

1. Go to **Settings** → **Environments**
2. Click **New environment**
3. Name: `Production`
4. Configure:
   - ✅ **Required reviewers**: Add yourself or team members
   - ✅ **Wait timer**: 0 minutes (or set delay)
   - ✅ **Deployment branches**: `main` only
5. Click **Save protection rules**

### Update Release Workflow

Edit `.github/workflows/release-provenance.yml`:

```yaml
jobs:
  publish-api:
    runs-on: ubuntu-latest
    environment: Production  # Add this line
    # ... rest of job
```

---

## 8. Verify SLSA Provenance

After your first release (e.g., `v0.1.0`), verify attestations:

```bash
# Install gh CLI if not already installed
# https://cli.github.com/

# Verify release artifact attestation
gh attestation verify app2-api-v0.1.0.zip \
  --owner Ira2222 \
  --repo App2

# Verify container image attestation
gh attestation verify oci://ghcr.io/ira2222/app2:v0.1.0 \
  --owner Ira2222 \
  --repo App2
```

Expected output:
```
✓ Verification succeeded!

Attestation subject:
  digest: sha256:abc123...
  name: app2-api-v0.1.0.zip

Attestation policy:
  statement: https://slsa.dev/provenance/v1
  predicate type: https://slsa.dev/provenance/v1
```

---

## 9. GitHub Actions Badge Status

After workflows run successfully, the README badges will show status:

- [![CI](https://github.com/Ira2222/App2/actions/workflows/ci.yml/badge.svg)](https://github.com/Ira2222/App2/actions/workflows/ci.yml) - Build and test status
- [![Security](https://github.com/Ira2222/App2/actions/workflows/security.yml/badge.svg)](https://github.com/Ira2222/App2/actions/workflows/security.yml) - Security scanning status
- [![Container](https://github.com/Ira2222/App2/actions/workflows/container.yml/badge.svg)](https://github.com/Ira2222/App2/actions/workflows/container.yml) - Docker build status
- [![Release (provenance)](https://github.com/Ira2222/App2/actions/workflows/release-provenance.yml/badge.svg)](https://github.com/Ira2222/App2/actions/workflows/release-provenance.yml) - Release status

---

## 10. Next Steps

### Run Your First Release

Once all workflows are green:

```bash
# Option 1: Full release workflow (recommended for first release)
./scripts/workflow-1-full-release.sh

# Option 2: Quick tag (if CI is already green)
./scripts/workflow-2-quick-tag.sh v0.1.0

# Option 3: Hotfix for specific commit
./scripts/workflow-3-hotfix-tag.sh v0.1.1 abc123
```

### Generate OpenAPI Client

```bash
# Generate TypeScript client for React app
./scripts/generate-openapi-client.sh
```

### Pull and Run Docker Image

```bash
# Pull from GHCR
docker pull ghcr.io/ira2222/app2:v0.1.0

# Run container
docker run --rm -p 5080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  ghcr.io/ira2222/app2:v0.1.0

# Test the API
curl http://localhost:5080/healthz/live
curl http://localhost:5080/api/todos
```

---

## Troubleshooting

### Workflows Not Running

**Symptom:** No workflows appear in Actions tab

**Solutions:**
1. Check that workflows exist in `.github/workflows/`
2. Ensure workflows have proper `on:` triggers
3. Verify Actions are enabled: Settings → Actions → General → "Allow all actions and reusable workflows"

### Permission Denied Errors

**Symptom:** `Error: Resource not accessible by integration`

**Solutions:**
1. Verify "Read and write permissions" is enabled (Step 1)
2. Check workflow permissions in each workflow file
3. Ensure `GITHUB_TOKEN` has required scopes

### Container Push Failures

**Symptom:** `Error: failed to push: access forbidden`

**Solutions:**
1. Ensure "Read and write permissions" is enabled
2. Verify package doesn't exist with different visibility
3. Check that repository and package names match

### Attestation Verification Failures

**Symptom:** `Error: attestation verification failed`

**Solutions:**
1. Ensure release workflow completed successfully
2. Check that artifacts were uploaded
3. Verify gh CLI is authenticated: `gh auth status`
4. Wait a few minutes after release for attestations to propagate

---

## Summary Checklist

Before running your first release, ensure:

- [ ] **Actions permissions**: Read and write enabled
- [ ] **CI workflow**: Passing (green checkmark)
- [ ] **Security workflow**: Passing (or findings reviewed)
- [ ] **Container workflow**: Passing (image pushed to GHCR)
- [ ] **Dependabot**: Enabled and configured
- [ ] **Branch protection**: (Optional) Configured for `main`
- [ ] **Secrets**: (Optional) Configured if using Azure Key Vault
- [ ] **Environments**: (Optional) Production environment created

---

## Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [GitHub Container Registry Documentation](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry)
- [SLSA Provenance](https://slsa.dev/provenance/)
- [GitHub CLI Documentation](https://cli.github.com/manual/)
- [Dependabot Configuration](https://docs.github.com/en/code-security/dependabot/dependabot-version-updates/configuration-options-for-the-dependabot.yml-file)

---

**Last Updated:** 2025-10-27
**Repository:** https://github.com/Ira2222/App2
