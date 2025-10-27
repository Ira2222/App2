# App2 Release Workflows Setup Guide

## Quick Start

### 1. Prerequisites Check
```bash
./scripts/check-prerequisites.sh
```

### 2. Configure GitHub Repository
```bash
# Set up remote (replace with your actual repo)
git remote add origin git@github.com:Ira2222/App2.git
git branch -M main
git push -u origin main
```

### 3. Run Full Release Cycle
```bash
./scripts/workflow-1-full-release.sh
```

---

## Detailed Setup Steps

### Step 1: GitHub Repository Configuration

#### A. Create GitHub Repository
1. Go to [GitHub](https://github.com/new)
2. Create repository named `App2`
3. **Do not** initialize with README, .gitignore, or license (we already have these)

#### B. Configure Repository Settings
1. **Settings → Actions → General**
   - **Workflow permissions**: Select "Read and write permissions"
   - **Allow GitHub Actions to create and approve pull requests**: Check this box

2. **Settings → Actions → Workflow permissions**
   - Ensure the repository has proper permissions for attestations

### Step 2: Local Git Setup

```bash
# Initialize and configure git (if not already done)
git init
git add -A
git commit -m "Initial commit: App2 with CI/CD release workflows"

# Add remote origin
git remote add origin git@github.com:Ira2222/App2.git

# Set main branch and push
git branch -M main
git push -u origin main
```

### Step 3: GitHub CLI Authentication

```bash
# Check if authenticated
gh auth status

# If not authenticated, login
gh auth login
```

### Step 4: Verify Prerequisites

```bash
./scripts/check-prerequisites.sh
```

Expected output:
```
✅ Git: git version 2.50.1
✅ GitHub CLI: gh version 2.82.0
✅ .NET: 8.0.121
✅ Docker: Docker version 28.5.1
✅ Repository: git@github.com:Ira2222/App2.git
✅ Project: App2 solution found
✅ Scripts: All workflow scripts present
```

### Step 5: Test the Workflows

#### A. Run Full Release Cycle
```bash
./scripts/workflow-1-full-release.sh
```

This will:
1. Run tests in Testing environment
2. Push to main branch
3. Wait for CI to complete
4. Create and push tag v0.1.0
5. Verify release attestation
6. Verify GHCR image provenance

#### B. Manual Verification (Optional)
```bash
# Verify release attestation
gh release verify v0.1.0 -R Ira2222/App2

# Verify specific asset (if you downloaded it)
gh release verify-asset v0.1.0 ./artifacts/app2-api.zip -R Ira2222/App2

# Verify container image
docker login ghcr.io
gh attestation verify oci://ghcr.io/Ira2222/App2:v0.1.0 -R Ira2222/App2
```

---

## Workflow Scripts Overview

### 1. Full Release Cycle (`workflow-1-full-release.sh`)
**Use when:** Initial release or major version bump

**What it does:**
- Runs full test suite locally
- Pushes to main branch
- Waits for CI to complete
- Creates annotated tag
- Verifies release and container attestations

### 2. Quick Tag (`workflow-2-quick-tag.sh`)
**Use when:** Tests already passed, CI already green

**What it does:**
- Creates annotated tag at HEAD
- Pushes tag
- Verifies release attestation

### 3. Hotfix Tag (`workflow-3-hotfix-tag.sh`)
**Use when:** Need to tag specific commit for hotfix

**What it does:**
- Creates annotated tag at specified commit
- Pushes tag
- Verifies release attestation

---

## Configuration

### Script Configuration
Edit the config variables at the top of each script:

```bash
# In workflow-1-full-release.sh, workflow-2-quick-tag.sh, workflow-3-hotfix-tag.sh
OWNER="Ira2222"                         # Your GitHub username/org
REPO="App2"                             # Repository name
TAG="v0.1.0"                           # Release tag
```

### Environment Variables
The scripts use these environment variables for testing:

```bash
export ASPNETCORE_ENVIRONMENT=Testing
export Features__KeyVault=false
export Features__Authentication=false
export Features__RedisOutputCache=false
```

---

## CI/CD Integration

### GitHub Actions Workflows

1. **CI Workflow** (`.github/workflows/ci.yml`)
   - Runs on push to main and PRs
   - Builds and tests .NET application
   - Builds web application
   - Generates SBOM with attestations

2. **Container Workflow** (`.github/workflows/container.yml`)
   - Builds and pushes Docker image to GHCR
   - Generates SLSA provenance attestations
   - Runs on push to main and releases

3. **Release Workflow** (`.github/workflows/release-provenance.yml`)
   - Creates GitHub releases with artifacts
   - Generates build provenance attestations
   - Verifies release and container attestations
   - Triggered by tag creation

4. **Security Workflow** (`.github/workflows/security.yml`)
   - Runs security scans
   - Generates security reports

### Permissions Required

The workflows require these permissions:

```yaml
permissions:
  contents: read          # Read repository contents
  packages: write         # Push to GHCR
  attestations: write     # Create attestations
  id-token: write         # Generate OIDC tokens
```

---

## Troubleshooting

### Common Issues

#### 1. GitHub CLI Not Authenticated
```bash
gh auth status
# If not authenticated:
gh auth login
```

#### 2. Repository Not Found
```bash
# Check remote URL
git remote get-url origin

# Update if needed
git remote set-url origin git@github.com:Ira2222/App2.git
```

#### 3. CI Workflow Fails
- Check GitHub Actions tab for error details
- Ensure repository has proper permissions
- Verify environment variables in CI workflow

#### 4. Attestation Verification Fails
- Ensure repository is public (attestations require GitHub Enterprise Cloud for private repos)
- Check that workflows have proper permissions
- Verify tag exists and release was created

#### 5. Container Image Not Found
- Check if container workflow ran successfully
- Verify GHCR package exists: `https://github.com/Ira2222/App2/pkgs/container/App2`
- Ensure Docker is logged in to GHCR

### Debug Commands

```bash
# Check GitHub CLI authentication
gh auth status

# List recent workflow runs
gh run list -R Ira2222/App2

# Watch specific workflow run
gh run watch <run-id> -R Ira2222/App2

# Check release details
gh release view v0.1.0 -R Ira2222/App2

# Verify release attestation
gh release verify v0.1.0 -R Ira2222/App2 --format json

# Check container image
docker manifest inspect ghcr.io/Ira2222/App2:v0.1.0
```

---

## Next Steps

### 1. First Release
1. Run `./scripts/workflow-1-full-release.sh`
2. Monitor GitHub Actions
3. Verify release at `https://github.com/Ira2222/App2/releases`
4. Check container at `https://github.com/Ira2222/App2/pkgs/container/App2`

### 2. Enable Redis Caching (Optional)
```bash
# Start Redis locally
docker compose -f docker/compose.redis.yml up -d

# Update production config
# Set Features__RedisOutputCache=true in appsettings.Production.json
```

### 3. Configure Azure AD Authentication
1. Create Azure AD app registration
2. Update `appsettings.Production.json` with real values
3. Enable `Features__Authentication=true` in production

### 4. Monitor and Maintain
- Review GitHub Actions runs regularly
- Update dependencies with Dependabot
- Monitor security alerts
- Verify attestations periodically

---

## Support

- **GitHub Issues**: Create issues in the repository
- **Documentation**: Check `scripts/README.md` for detailed workflow documentation
- **CI/CD**: Monitor `.github/workflows/` for workflow status
- **Security**: Review security alerts in GitHub Security tab
