#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Create a new project from the App2 template with full automation.

.DESCRIPTION
    Creates a new GitHub repository from the App2 template and automatically configures:
    - Branch protection rules requiring CI checks and PR reviews
    - Environments (dev, staging, prod) with branch policies
    - Security features (secret scanning, push protection, Dependabot)
    - CodeQL code scanning with security-extended queries

.PARAMETER Name
    The name of the new repository (e.g., "LocationForm")

.PARAMETER Owner
    The GitHub owner/organization (default: "Ira2222")

.PARAMETER Private
    Create as a private repository (default: true)

.PARAMETER Description
    Repository description (default: "FIN: {Name} (created from App2 template)")

.EXAMPLE
    pwsh ./scripts/new-project.ps1 -Name LocationForm

.EXAMPLE
    pwsh ./scripts/new-project.ps1 -Name LocationForm -Owner MyOrg -Private:$false
#>

param(
  [Parameter(Mandatory=$true)]
  [string]$Name,

  [string]$Owner = "Ira2222",

  [switch]$Private = $true,

  [string]$Description = "FIN: $Name (created from App2 template)"
)

$ErrorActionPreference = "Stop"
$repo = "$Owner/$Name"
$visibility = if ($Private.IsPresent) { "--private" } else { "--public" }

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " App2 Template → New Project Automation" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Preflight checks
Write-Host "→ Checking prerequisites..." -ForegroundColor Yellow
try {
    gh --version | Out-Null
    Write-Host "  ✓ GitHub CLI installed" -ForegroundColor Green
} catch {
    Write-Host "  ✗ GitHub CLI (gh) not found. Install from: https://cli.github.com/" -ForegroundColor Red
    exit 1
}

try {
    gh auth status 2>&1 | Out-Null
    Write-Host "  ✓ GitHub CLI authenticated" -ForegroundColor Green
} catch {
    Write-Host "  ✗ GitHub CLI not authenticated. Run: gh auth login" -ForegroundColor Red
    exit 1
}

# Step 1: Create repo from template
Write-Host ""
Write-Host "→ Creating $repo from App2 template..." -ForegroundColor Yellow
try {
    gh repo create $repo --template "$Owner/App2" $visibility --clone 2>&1 | Out-Null
    Write-Host "  ✓ Repository created and cloned" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Failed to create repository: $_" -ForegroundColor Red
    exit 1
}

Set-Location "$Name"

# Step 2: Repository settings
Write-Host ""
Write-Host "→ Configuring repository settings..." -ForegroundColor Yellow
gh repo edit $repo --description "$Description" 2>&1 | Out-Null
gh api -X PATCH "repos/$repo" -H "X-GitHub-Api-Version: 2022-11-28" -f delete_branch_on_merge=true 2>&1 | Out-Null
Write-Host "  ✓ Auto-delete head branches enabled" -ForegroundColor Green

# Step 3: Security & analysis
Write-Host ""
Write-Host "→ Enabling security features..." -ForegroundColor Yellow
@"
{
  "security_and_analysis": {
    "secret_scanning": { "status": "enabled" },
    "secret_scanning_push_protection": { "status": "enabled" },
    "dependabot_security_updates": { "status": "enabled" }
  }
}
"@ | gh api -X PATCH "repos/$repo" -H "X-GitHub-Api-Version: 2022-11-28" --input - 2>&1 | Out-Null
Write-Host "  ✓ Secret scanning enabled" -ForegroundColor Green
Write-Host "  ✓ Push protection enabled" -ForegroundColor Green
Write-Host "  ✓ Dependabot security updates enabled" -ForegroundColor Green

# Step 4: Create environments
function New-Env {
    param([string]$envName)

    # Create environment with branch policy
    @"
{
  "deployment_branch_policy": {
    "protected_branches": false,
    "custom_branch_policies": true
  }
}
"@ | gh api -X PUT "repos/$repo/environments/$envName" -H "X-GitHub-Api-Version: 2022-11-28" --input - 2>&1 | Out-Null

    # Allow only main branch to deploy
    @"
{ "name": "main" }
"@ | gh api -X POST "repos/$repo/environments/$envName/deployment-branch-policies" -H "X-GitHub-Api-Version: 2022-11-28" --input - 2>&1 | Out-Null
}

Write-Host ""
Write-Host "→ Creating environments..." -ForegroundColor Yellow
New-Env -envName "dev"
Write-Host "  ✓ dev environment created (main branch only)" -ForegroundColor Green
New-Env -envName "staging"
Write-Host "  ✓ staging environment created (main branch only)" -ForegroundColor Green
New-Env -envName "prod"
Write-Host "  ✓ prod environment created (main branch only)" -ForegroundColor Green

# Step 5: Branch protection
Write-Host ""
Write-Host "→ Configuring branch protection on main..." -ForegroundColor Yellow
@"
{
  "required_status_checks": {
    "strict": true,
    "contexts": ["ci"]
  },
  "enforce_admins": true,
  "required_pull_request_reviews": {
    "required_approving_review_count": 1,
    "dismiss_stale_reviews": true,
    "require_code_owner_reviews": false
  },
  "restrictions": null,
  "allow_force_pushes": false,
  "allow_deletions": false,
  "required_conversation_resolution": true
}
"@ | gh api -X PUT "repos/$repo/branches/main/protection" -H "X-GitHub-Api-Version: 2022-11-28" --input - 2>&1 | Out-Null
Write-Host "  ✓ Require CI to pass" -ForegroundColor Green
Write-Host "  ✓ Require 1 PR review" -ForegroundColor Green
Write-Host "  ✓ Dismiss stale reviews" -ForegroundColor Green
Write-Host "  ✓ Require conversation resolution" -ForegroundColor Green
Write-Host "  ✓ Disable force pushes and deletions" -ForegroundColor Green

# Step 6: CodeQL default setup
Write-Host ""
Write-Host "→ Enabling CodeQL scanning..." -ForegroundColor Yellow
@"
{
  "state": "enabled",
  "query_suite": "security-extended",
  "languages": ["csharp","javascript","typescript"]
}
"@ | gh api -X PATCH "repos/$repo/code-scanning/default-setup" -H "X-GitHub-Api-Version: 2022-11-28" --input - 2>&1 | Out-Null
Write-Host "  ✓ CodeQL enabled (security-extended)" -ForegroundColor Green
Write-Host "  ✓ Languages: C#, JavaScript, TypeScript" -ForegroundColor Green

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " ✓ Repository Ready!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Repository: https://github.com/$repo" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Update CODEOWNERS (.github/CODEOWNERS)" -ForegroundColor White
Write-Host "  2. Rename namespaces (App2.* → $Name.*)" -ForegroundColor White
Write-Host "  3. Replace Todo entity with your domain entities" -ForegroundColor White
Write-Host "  4. Update README.md and appsettings.json" -ForegroundColor White
Write-Host "  5. Make first commit to trigger CI" -ForegroundColor White
Write-Host ""
Write-Host "Trigger CI:" -ForegroundColor Yellow
Write-Host '  echo "init" >> README.md' -ForegroundColor Gray
Write-Host '  git add README.md && git commit -m "chore: init" && git push' -ForegroundColor Gray
Write-Host '  gh run watch' -ForegroundColor Gray
Write-Host ""
